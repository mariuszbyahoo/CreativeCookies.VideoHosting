﻿using CreativeCookies.VideoHosting.Contracts.Enums;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.Repositories.OAuth;
using CreativeCookies.VideoHosting.Contracts.Services.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Web;
using CreativeCookies.VideoHosting.Contracts.Services.IdP;
using CreativeCookies.VideoHosting.DTOs.OAuth;

namespace CreativeCookies.VideoHosting.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IOAuthClientService _oAuthClientService;
        private readonly IAuthorizationCodeService _codesService;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IAccessTokenService _accessTokenService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRefreshTokenService _refreshTokenSrv;
        private readonly IMyHubSignInManager _signInManager;
        private readonly IMyHubUserManager _userManager;

        public const string CacheControlHeader = "no-store";
        public const string RefreshTokenGrantType = "refresh_token";
        public const string AuthorizationCodeGrantType = "authorization_code";
        public const string UnsupportedGrantType = "unsupported_grant_type";
        public const string ServerError = "server_error";
        private readonly string _jwtSecretKey;
        private readonly string _apiUrl;

        public AuthController(IOAuthClientService oAuthClientService, IAuthorizationCodeService codesService, IAccessTokenService accessTokenService,
            ILogger<AuthController> logger, IConfiguration configuration, IHttpContextAccessor httpContextAccessor,
            IRefreshTokenService refreshTokenSrv, IMyHubSignInManager signInManager,
            IMyHubUserManager userManager)
        {
            _oAuthClientService = oAuthClientService;
            _codesService = codesService;
            _logger = logger;
            _configuration = configuration;
            _accessTokenService = accessTokenService;
            _httpContextAccessor = httpContextAccessor;
            _refreshTokenSrv = refreshTokenSrv;
            _signInManager = signInManager;
            _userManager = userManager;
            _jwtSecretKey = _configuration.GetValue<string>("JWTSecretKey");
            _apiUrl = _configuration.GetValue<string>("ApiUrl");
        }

        /// <summary>
        /// Method checks ltrt or stac cookies are available and not expired, if so - it logs user in
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        [HttpGet("isAuthenticated")]
        public async Task<IActionResult> CheckAuthentication(string clientId)
        {
            var stac = Request.Cookies["stac"];
            string userEmail = null;
            string userRole = null;

            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _apiUrl,
                ValidAudience = clientId.ToString(),
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecretKey)),
            }; // HACK: hardcoded values above prohibits usage of any other external IdPs 

            if (tokenHandler.CanReadToken(stac))
            {
                try
                {
                    var claimsPrincipal = tokenHandler.ValidateToken(stac, validationParameters, out _);
                    userEmail = claimsPrincipal.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Email))?.Value;
                    userRole = claimsPrincipal.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Role))?.Value;
                }
                catch (SecurityTokenExpiredException)
                {
                    var result = await HandleRefreshTokenGrant(clientId);

                    if (result is OkObjectResult okResult)
                    {
                        var data = okResult.Value as dynamic;
                        if (data != null)
                        {
                            try
                            {
                                var claimsPrincipal = tokenHandler.ValidateToken((string)data.access_token, validationParameters, out _);
                                userEmail = claimsPrincipal.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Email))?.Value;
                                userRole = claimsPrincipal.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Role))?.Value;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError("Exception occured while extracting the email from stac cookie's access_token");
                                return StatusCode(500, ServerError);
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(userEmail) && !string.IsNullOrEmpty(userRole))
            {
                if (!User.Identity.IsAuthenticated)
                {
                    var user = await _userManager.FindByEmailAsync(userEmail);
                    await _signInManager.SignInAsync(user, false);
                }
                return Ok(new { isAuthenticated = true, email = userEmail, userRole = userRole });
            }
            else
            {
                Response.Cookies.Delete("stac");
                Response.Cookies.Delete("ltrt");
                return Ok(new { isAuthenticated = false });
            }
        }


        [HttpGet("authorize")]
        public async Task<IActionResult> Authorize([FromQuery]string? client_id, [FromQuery] string? redirect_uri, 
            [FromQuery] string? response_type, [FromQuery] string? state,
            [FromQuery] string? code_challenge, [FromQuery] string? code_challenge_method)
        {
            try
            {
                HttpContext.Response.Headers["Cache-Control"] = "no-store";
                var validationResult = await ValidateAuthRequestParameters(redirect_uri, client_id, state, response_type, code_challenge, code_challenge_method);
                if (validationResult != null)
                {
                    return validationResult;
                }

                if (!User.Identity.IsAuthenticated)
                {
                    var returnUrl = $"/auth/authorize?client_id={client_id}&redirect_uri={redirect_uri}&response_type={response_type}&state={state}&code_challenge={code_challenge}&code_challenge_method={code_challenge_method}";
                    var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
                    var loginUrl = $"{baseUrl}/Identity/Account/Login?returnUrl={WebUtility.UrlEncode(returnUrl)}";

                    return Redirect(loginUrl);
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var authorizationCode = await _codesService.GenerateAuthorizationCode(client_id, userId, redirect_uri, code_challenge, code_challenge_method);

                var redirectUriBuilder = new UriBuilder(redirect_uri);
                var queryParameters = HttpUtility.ParseQueryString(redirectUriBuilder.Query);
                queryParameters["code"] = authorizationCode;
                queryParameters["state"] = state;
                redirectUriBuilder.Query = queryParameters.ToString();

                return Redirect(redirectUriBuilder.ToString());
            }
            catch(Exception ex)
            {
                _logger.LogError($"Unexpected exception when ran Authenticate call with params: {nameof(client_id)}: {client_id}, {nameof(redirect_uri)}: {redirect_uri}, {nameof(response_type)}: {response_type}, {nameof(state)}: {state}, {nameof(code_challenge)}: {code_challenge}, {nameof(code_challenge_method)}: {code_challenge_method}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "Unexpected error occured, try again later or if the issue persists, contact support");
            }
        }

        [HttpPost("token")]
        public async Task<IActionResult> Token([FromForm] string? grant_type, [FromForm] string? code, [FromForm] string? redirect_uri, [FromForm] string? client_id, [FromForm] string? code_verifier)
        {
            try
            {
                HttpContext.Response.Headers["Cache-Control"] = CacheControlHeader;

                switch (grant_type)
                {
                    case RefreshTokenGrantType:
                        return await HandleRefreshTokenGrant(client_id);

                    case AuthorizationCodeGrantType:
                        return await HandleAuthorizationCodeGrant(code, redirect_uri, client_id, code_verifier, grant_type);

                    default:
                        return GenerateBadRequest(UnsupportedGrantType);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected exception when ran Token call with params: {nameof(client_id)}: {client_id}, {nameof(redirect_uri)}: {redirect_uri}, {nameof(grant_type)}: {grant_type}, {nameof(code)}: {code}, {nameof(code_verifier)}: {code_verifier} , ex: {ex.Message}, stackTrace: {ex.StackTrace}, source: {ex.Source}, innerException: {ex.InnerException}");
                return RedirectToError(redirect_uri, ServerError);
            }
        }

        /// <summary>
        /// Logs user out and revokes the refreshToken 
        /// Duplicate of Logout.cshtml.cs from ASP.NET
        /// </summary>
        /// <param name="returnPath">relative path of an endpoint where to redirect after logout with initial slash, ex. "/films-list"</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [HttpPost]
        [Route("logout")]
        public async Task<IActionResult> Logout(string returnPath = "")
        {
            await _signInManager.SignOutAsync();
            var ltrt = "ltrt";
            var stac = "stac";
            var aspNetCoreAuthCookie = ".AspNetCore.Identity.Application";
            if (Request.Cookies.ContainsKey(ltrt))
            {
                var refreshToken = Request.Cookies[ltrt].ToString();
                await _refreshTokenSrv.RevokeRefreshToken(refreshToken);
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                };
                Response.Cookies.Delete(ltrt, cookieOptions);
            }
            if (Request.Cookies.ContainsKey(stac))
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                };
                Response.Cookies.Delete(stac, cookieOptions);
            }

            if (Request.Cookies.ContainsKey(aspNetCoreAuthCookie))
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                };
                Response.Cookies.Delete(aspNetCoreAuthCookie);
            }
            else return Ok();

            // HACK: Delete AspNetCore.Identity.Application cookie

            _logger.LogInformation("User logged out.");
            var afterLogoutRedirectUrl = _configuration["ClientUrl"];
            if (!string.IsNullOrWhiteSpace(afterLogoutRedirectUrl))
            {
                if (!string.IsNullOrWhiteSpace(returnPath))
                {
                    afterLogoutRedirectUrl += returnPath;
                }
                return Ok($"redirectTo={afterLogoutRedirectUrl}");
            }
            else
            {
                throw new ArgumentNullException("ClientUrl has not been found in the API's configuration");
            }
        }

        private async Task<IActionResult> HandleAuthorizationCodeGrant(string? code, string? redirect_uri, string? client_id, string? code_verifier, string grant_type)
        {
            var clientIdRedirectUrlErrorResponse = await ValidateRedirectUriAndClientId(redirect_uri, client_id, false);
            if (clientIdRedirectUrlErrorResponse != null)
            {
                return clientIdRedirectUrlErrorResponse;
            }
            var codeAndCodeVerifierErrorResponse = await ValidateCodeAndCodeVerifier(code, code_verifier, client_id);
            if (codeAndCodeVerifierErrorResponse != null)
            {
                return codeAndCodeVerifierErrorResponse;
            }

            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            var extractedUser = await _codesService.GetUserByAuthCodeAsync(code);
            var isAdmin = extractedUser.Role.Equals("admin", StringComparison.InvariantCultureIgnoreCase);
            if (!isAdmin)
                extractedUser = await CheckSubscriptionStatus(extractedUser);

            if (extractedUser == null)
            {
                _logger.LogError($"Codes repo returned null for GetUserByAuthCodeAsync when invoked inside Token action with params: {nameof(client_id)}: {client_id}, {nameof(redirect_uri)}: {redirect_uri}, {nameof(grant_type)}: {grant_type}, {nameof(code)}: {code}, {nameof(code_verifier)}: {code_verifier}");
                return GenerateBadRequest("server_error");
            }

            var accessToken = _accessTokenService.GetNewAccessToken(extractedUser.Id, extractedUser.UserEmail, Guid.Parse(client_id), _configuration, baseUrl, extractedUser.Role);
            var refreshToken = await _refreshTokenSrv.GetNewRefreshToken(extractedUser.Id);
            var accessTokenCookieOptions = ReturnAuthCookieOptions(1);
            _httpContextAccessor.HttpContext.Response.Cookies.Append("stac", accessToken, accessTokenCookieOptions);

            var refreshTokenCookieOptions = ReturnAuthCookieOptions(8);
            _httpContextAccessor.HttpContext.Response.Cookies.Append("ltrt", refreshToken.Token, refreshTokenCookieOptions);

            var response = Ok(new
            {
                access_token = accessToken,
                token_type = "Bearer",
                expires_in = 3600
            });
            return response;

        }

        private async Task<IActionResult> HandleRefreshTokenGrant(string? client_id)
        {
            var clientIdRedirectUrlErrorResponse = await ValidateClientId(client_id);
            if (clientIdRedirectUrlErrorResponse != null)
            {
                return clientIdRedirectUrlErrorResponse;
            }

            var refreshToken = Request.Cookies["ltrt"];
            if (!string.IsNullOrWhiteSpace(refreshToken) && await _refreshTokenSrv.IsTokenValid(refreshToken))
            {
                var user = await _refreshTokenSrv.GetUserByRefreshToken(refreshToken);

                if (user == null)
                {
                    return BadRequest("invalid refresh_token");
                }
                var isAdmin = user.Role.Equals("admin", StringComparison.InvariantCultureIgnoreCase);
                if (!isAdmin)
                    user = await CheckSubscriptionStatus(user);

                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";

                var newAccessToken = _accessTokenService.GetNewAccessToken(user.Id, user.UserEmail, Guid.Parse(client_id), _configuration, baseUrl, user.Role);
                var newRefreshToken = await _refreshTokenSrv.GetNewRefreshToken(user.Id);

                var accessTokenCookieOptions = ReturnAuthCookieOptions(1);
                _httpContextAccessor.HttpContext.Response.Cookies.Append("stac", newAccessToken, accessTokenCookieOptions);

                var refreshTokenCookieOptions = ReturnAuthCookieOptions(8);
                _httpContextAccessor.HttpContext.Response.Cookies.Append("ltrt", newRefreshToken.Token, refreshTokenCookieOptions);

                var response = Ok(new
                {
                    access_token = newAccessToken,
                    token_type = "Bearer",
                    expires_in = 3600 
                });
                return response;
            }
            return BadRequest("invalid refresh_token");
        }

        private async Task<MyHubUserDto> CheckSubscriptionStatus(MyHubUserDto user)
        {
            if (user.SubscriptionStartDateUTC < DateTime.UtcNow && user.SubscriptionEndDateUTC > DateTime.UtcNow) // HACK: Stripe has 1,5 hr delay between subscription end date and this.
            {
                await _userManager.AddToRoleAsync(user, "subscriber");
                await _userManager.RemoveFromRoleAsync(user, "nonsubscriber");
                user.Role = "subscriber";
            }
            else
            {
                await _userManager.RemoveFromRoleAsync(user, "subscriber");
                await _userManager.AddToRoleAsync(user, "nonsubscriber");
                user.Role = "nonsubscriber";
            }
            return user;
        }

        private async Task<IActionResult?> ValidateCodeAndCodeVerifier(string code, string code_verifier, string client_id)
        {
            var verificationResponse = await _oAuthClientService.IsCodeWithVerifierValid(code_verifier, code, client_id);
            switch (verificationResponse)
            {
                case OAuthErrorResponse.InvalidRequest:
                    return GenerateBadRequest("invalid_request");
                case OAuthErrorResponse.InvalidGrant:
                    return GenerateBadRequest("invalid_grant");
                case null: // positive route
                    return null;
                default: 
                    _logger.LogError($"Unexpected OAuth error response with params: {nameof(client_id)}: {client_id}, {nameof(code)}: {code}, {nameof(code_verifier)}: {code_verifier}");
                    return GenerateBadRequest("server_error");

            }
        }

        private async Task<IActionResult?> ValidateAuthRequestParameters(string redirect_uri, string client_id, string state, string response_type, string code_challenge, string code_challenge_method)
        {
            var clientIdRedirectUrlErrorResponse = await ValidateRedirectUriAndClientId(redirect_uri, client_id, true, state);
            if (clientIdRedirectUrlErrorResponse != null)
            {
                return clientIdRedirectUrlErrorResponse;
            }
            if (string.IsNullOrWhiteSpace(response_type)) return RedirectToError(redirect_uri, "unsupported_response_type", state);
            if (string.IsNullOrWhiteSpace(state)) return RedirectToError(redirect_uri, "invalid_request", state);
            if (string.IsNullOrWhiteSpace(code_challenge)) return RedirectToError(redirect_uri, "invalid_request", state); 
            if (string.IsNullOrWhiteSpace(code_challenge_method)) return RedirectToError(redirect_uri, "invalid_request", state); 
            if (!response_type.Equals("code")) return RedirectToError(redirect_uri, "invalid_request", state);

            // all good
            return null;
        }
        private async Task<IActionResult?> ValidateClientId(string client_id)
        {
            var clientIdError = await ValidateClientIdInternal(client_id);
            if (clientIdError != null && clientIdError.HasValue)
            {
                var errorResponse = string.Empty;
                switch (clientIdError.Value)
                {
                    case OAuthErrorResponse.InvalidRequest:
                        _logger.LogDebug($"Received invalid request with params: {nameof(client_id)}: {client_id}");
                        errorResponse = "invalid_request";
                        return GenerateBadRequest(errorResponse);
                    case OAuthErrorResponse.UnauthorisedClient:
                        _logger.LogDebug($"Received invalid request with an unauthorised client, and params: {nameof(client_id)}: {client_id}");
                        errorResponse = "unauthorised_client";
                        return GenerateBadRequest(errorResponse);
                    default:
                        _logger.LogError($"Unexpected OAuth error response with params: {nameof(client_id)}: {client_id}", clientIdError.Value);
                        errorResponse = "server_error";
                        return GenerateBadRequest(errorResponse);
                }
            }

            // all good, no errors to return 
            return null;
        }
        private async Task<IActionResult?> ValidateRedirectUriAndClientId(
            string redirect_uri, string client_id, bool redirectsErrorsToClientApp = true ,string state = "")
        {
            var redirectUriError = await ValidateRedirectUriInternal(redirect_uri);
            if (redirectUriError != null && redirectUriError.HasValue)
            {
                var errorResponse = string.Empty;
                switch (redirectUriError.Value)
                {
                    case OAuthErrorResponse.InvalidRedirectUri:
                        _logger.LogDebug($"Received invalid request with an invalid redirect_uri, and params: {nameof(client_id)}: {client_id}, {nameof(redirect_uri)}: {redirect_uri}");
                        return GenerateBadRequest("Invalid redirect_uri");
                    case OAuthErrorResponse.InvalidRequest:
                        _logger.LogDebug($"Received invalid request with params: {nameof(client_id)}: {client_id}, {nameof(redirect_uri)}: {redirect_uri}");
                        errorResponse = "invalid_request";
                        if (redirectsErrorsToClientApp) return RedirectToError(redirect_uri, errorResponse, state);
                        else return GenerateBadRequest(errorResponse);
                    default:
                        _logger.LogError($"Unexpected OAuth error response with params: {nameof(client_id)}: {client_id}, {nameof(redirect_uri)}: {redirect_uri}", redirectUriError.Value);
                        errorResponse = "server_error";
                        if (redirectsErrorsToClientApp)
                        {
                            return RedirectToError(redirect_uri, errorResponse, state);
                        }
                        else 
                        {
                            return GenerateBadRequest(errorResponse);
                        }
                }
            }
            var clientIdError = await ValidateClientIdInternal(client_id);
            if (clientIdError != null && clientIdError.HasValue)
            {
                var errorResponse = string.Empty;
                switch (clientIdError.Value)
                {
                    case OAuthErrorResponse.InvalidRequest:
                        _logger.LogDebug($"Received invalid request with params: {nameof(client_id)}: {client_id}, {nameof(redirect_uri)}: {redirect_uri}");
                        errorResponse = "invalid_request";
                        return GenerateBadRequest(errorResponse);
                    case OAuthErrorResponse.UnauthorisedClient:
                        _logger.LogDebug($"Received invalid request with an unauthorised client, and params: {nameof(client_id)}: {client_id}, {nameof(redirect_uri)}: {redirect_uri}");
                        errorResponse = "unauthorised_client";
                        return GenerateBadRequest(errorResponse);
                    default:
                        _logger.LogError($"Unexpected OAuth error response with params: {nameof(client_id)}: {client_id}, {nameof(redirect_uri)}: {redirect_uri}", clientIdError.Value);
                        errorResponse = "server_error";
                        return GenerateBadRequest(errorResponse);
                }
            }

            if (await _oAuthClientService.WasRedirectUriRegisteredToClient(redirect_uri, client_id))
            {
                // all good, no errors to return 
                return null;
            }
            else return GenerateBadRequest("Invalid redirect_uri");
        }
        private async Task<OAuthErrorResponse?> ValidateClientIdInternal(string inputClientId)
        {
            Guid clientId;
            var parsedSuccessfully = Guid.TryParse(inputClientId, out clientId);
            if (string.IsNullOrWhiteSpace(inputClientId) || !parsedSuccessfully)
            {
                return OAuthErrorResponse.InvalidRequest;
            }
            if (parsedSuccessfully && Guid.Empty != clientId)
            {
                var lookup = await _oAuthClientService.FindByClientIdAsync(clientId);
                if (lookup == null) return OAuthErrorResponse.UnauthorisedClient;
            }
            return null;
        }
        private async Task<OAuthErrorResponse?> ValidateRedirectUriInternal(string inputRedirectUri)
        {
            if (!string.IsNullOrWhiteSpace(inputRedirectUri) && await _oAuthClientService.IsRedirectUriPresentInDatabase(inputRedirectUri))
            {
                return null;
            }

            return OAuthErrorResponse.InvalidRedirectUri;
        }
        private IActionResult RedirectToError(string redirectUri, string error, string state = "", string errorDescription = "")
        {
            var uriBuilder = new UriBuilder(redirectUri);
            var queryParameters = HttpUtility.ParseQueryString(uriBuilder.Query);

            queryParameters["error"] = error;
            if (!string.IsNullOrWhiteSpace(errorDescription))
            {
                queryParameters["error_description"] = errorDescription;
            }
            if (!string.IsNullOrWhiteSpace(state))
            {
                queryParameters["state"] = state;
            }
            uriBuilder.Query = queryParameters.ToString();

            return Redirect(uriBuilder.ToString());
        }

        private IActionResult GenerateBadRequest(string error)
        {
            var errorResponse = new Dictionary<string, string> { { "error", error } };

            HttpContext.Response.Headers["Cache-Control"] = "no-store";

            return BadRequest(errorResponse);
        }

        private CookieOptions ReturnAuthCookieOptions(double hoursToExpire)
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(hoursToExpire)
            };
        }

    }
}
