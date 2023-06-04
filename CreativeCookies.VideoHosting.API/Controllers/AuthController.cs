using CreativeCookies.VideoHosting.Contracts.Enums;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.Repositories.OAuth;
using CreativeCookies.VideoHosting.DAL.DAOs.OAuth;
using CreativeCookies.VideoHosting.Domain.DTOs.OAuth;
using CreativeCookies.VideoHosting.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Net;
using System.Security.Claims;
using System.Web;

namespace CreativeCookies.VideoHosting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IClientStore _store;
        private readonly IAuthorizationCodeRepository _codesRepo;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IJWTRepository _jwtRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public const string CacheControlHeader = "no-store";
        public const string RefreshTokenGrantType = "refresh_token";
        public const string AuthorizationCodeGrantType = "authorization_code";
        public const string UnsupportedGrantType = "unsupported_grant_type";
        public const string ServerError = "server_error";

        public AuthController(IClientStore store, IAuthorizationCodeRepository codesRepo, IJWTRepository jwtRepository, 
            ILogger<AuthController> logger, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IRefreshTokenRepository refreshTokenRepository)
        {
            _store = store;
            _codesRepo = codesRepo;
            _logger = logger;
            _configuration = configuration;
            _jwtRepository = jwtRepository;
            _httpContextAccessor = httpContextAccessor;
            _refreshTokenRepository = refreshTokenRepository;
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
                    var returnUrl = $"/api/auth/authorize?client_id={client_id}&redirect_uri={redirect_uri}&response_type={response_type}&state={state}&code_challenge={code_challenge}&code_challenge_method={code_challenge_method}";
                    var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
                    var loginUrl = $"{baseUrl}/Identity/Account/Login?returnUrl={WebUtility.UrlEncode(returnUrl)}";

                    return Redirect(loginUrl);
                }

                // Optional - display a screen to get user's permissions (if necessary)

                var authorizationCode = await _codesRepo.GetAuthorizationCode(client_id, User.FindFirstValue(ClaimTypes.NameIdentifier), redirect_uri, code_challenge, code_challenge_method);

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
                        return await HandleRefreshTokenGrant(code, client_id);

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

            var extractedUser = await _codesRepo.GetUserByAuthCodeAsync(code);
            if (extractedUser == null)
            {
                _logger.LogError($"Codes repo returned null for GetUserByAuthCodeAsync when invoked inside Token action with params: {nameof(client_id)}: {client_id}, {nameof(redirect_uri)}: {redirect_uri}, {nameof(grant_type)}: {grant_type}, {nameof(code)}: {code}, {nameof(code_verifier)}: {code_verifier}");
                return GenerateBadRequest("server_error");
            }

            var accessToken = _jwtRepository.GenerateAccessToken(extractedUser.Id, extractedUser.UserEmail, Guid.Parse(client_id), _configuration, baseUrl);
            var refreshToken = await _refreshTokenRepository.CreateRefreshToken(extractedUser.Id);
            // HACK: TODO implement RBAC as describen in RFC6749 3.3

            var response = Ok(new
            {
                access_token = accessToken,
                refresh_token = refreshToken.Token,
                token_type = "Bearer",
                expires_in = 3600
            });
            return response;

        }

        private async Task<IActionResult> HandleRefreshTokenGrant(string? refresh_token, string? client_id)
        {
            // Validate the client ID and redirect URI.
            var clientIdRedirectUrlErrorResponse = await ValidateClientId(client_id);
            if (clientIdRedirectUrlErrorResponse != null)
            {
                return clientIdRedirectUrlErrorResponse;
            }

            // Find the user by the refresh token
            var user = await _refreshTokenRepository.GetUserByRefreshToken(refresh_token);

            if (user == null)
            {
                // The refresh token is invalid or has expired
                throw new NotImplementedException("Implement all actions to do if a refresh token is invalid or has expired");
            }

            // Get the request's base URL.
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";

            // Generate a new access token and refresh token for the user.
            var newAccessToken = _jwtRepository.GenerateAccessToken(user.Id, user.UserEmail, Guid.Parse(client_id), _configuration, baseUrl);
            var newRefreshToken = await _refreshTokenRepository.CreateRefreshToken(user.Id);

            // Return the new tokens.
            var response = Ok(new
            {
                access_token = newAccessToken,
                refresh_token = newRefreshToken.Token,
                token_type = "Bearer",
                expires_in = 3600 // Adjust this according to your needs
            });
            return response;
        }

        private async Task<IActionResult?> ValidateCodeAndCodeVerifier(string code, string code_verifier, string client_id)
        {
            var verificationResponse = await _store.IsCodeWithVerifierValid(code_verifier, code, client_id);
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
        public async Task<IActionResult?> ValidateClientId(string client_id)
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

            if (await _store.WasRedirectUriRegisteredToClient(redirect_uri, client_id))
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
                var lookup = await _store.FindByClientIdAsync(clientId);
                if (lookup == null) return OAuthErrorResponse.UnauthorisedClient;
            }
            return null;
        }
        private async Task<OAuthErrorResponse?> ValidateRedirectUriInternal(string inputRedirectUri)
        {
            if (!string.IsNullOrWhiteSpace(inputRedirectUri) && await _store.IsRedirectUriPresentInDatabase(inputRedirectUri))
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

    }
}
