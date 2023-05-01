using CreativeCookies.VideoHosting.Contracts.Enums;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.Repositories.OAuth;
using CreativeCookies.VideoHosting.DAL.DAOs.OAuth;
using CreativeCookies.VideoHosting.Domain.DTOs.OAuth;
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

        public AuthController(IClientStore store, IAuthorizationCodeRepository codesRepo, IJWTRepository jwtRepository, 
            ILogger<AuthController> logger, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _store = store;
            _codesRepo = codesRepo;
            _logger = logger;
            _configuration = configuration;
            _jwtRepository = jwtRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("authorize")]
        public async Task<IActionResult> Authorize([FromQuery]string? client_id, [FromQuery] string? redirect_uri, 
            [FromQuery] string? response_type, [FromQuery] string? scope, [FromQuery] string? state,
            [FromQuery] string? code_challenge, [FromQuery] string? code_challenge_method)
        {
            try
            {
                HttpContext.Response.Headers["Cache-Control"] = "no-store";
                var validationResult = await ValidateAuthRequestParameters(redirect_uri, client_id, state, response_type, scope, code_challenge, code_challenge_method);
                if (validationResult != null)
                {
                    return validationResult;
                }

                if (!User.Identity.IsAuthenticated)
                {
                    var returnUrl = $"/api/auth/authorize?client_id={client_id}&redirect_uri={redirect_uri}&response_type={response_type}&scope={scope}&state={state}&code_challenge={code_challenge}&code_challenge_method={code_challenge_method}";
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
                _logger.LogError($"Unexpected exception when ran Authenticate call with params: {nameof(client_id)}: {client_id}, {nameof(redirect_uri)}: {redirect_uri}, {nameof(response_type)}: {response_type}, {nameof(scope)}: {scope}, {nameof(state)}: {state}, {nameof(code_challenge)}: {code_challenge}, {nameof(code_challenge_method)}: {code_challenge_method}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "Unexpected error occured, try again later or if the issue persists, contact support");
            }
        }

        [HttpPost("token")]
        public async Task<IActionResult> Token([FromForm] string? grant_type, [FromForm] string? code, [FromForm] string? redirect_uri, [FromForm] string? client_id, [FromForm] string? code_verifier)
        {
            try
            {
                HttpContext.Response.Headers["Cache-Control"] = "no-store";
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
                if (string.IsNullOrWhiteSpace(grant_type) || (!string.IsNullOrWhiteSpace(grant_type) && !grant_type.Equals("authorization_code")))
                {
                    return GenerateBadRequest("unsupported_grant_type");
                }

                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";

                var extractedUser = await _codesRepo.GetUserByAuthCodeAsync(code);
                if(extractedUser == null)
                {
                    _logger.LogError($"Codes repo returned null for GetUserByAuthCodeAsync when invoked inside Token action with params: {nameof(client_id)}: {client_id}, {nameof(redirect_uri)}: {redirect_uri}, {nameof(grant_type)}: {grant_type}, {nameof(code)}: {code}, {nameof(code_verifier)}: {code_verifier}");
                    return GenerateBadRequest("server_error");
                }

                var access_token = _jwtRepository.GenerateAccessToken(extractedUser.Id, extractedUser.UserEmail, Guid.Parse(client_id), _configuration, baseUrl);

                // HACK: Add scopes for the GenerateAccessToken in order to implement RBAC as describen in RFC6749 3.3

                var response = Ok(new
                {
                    access_token = access_token,
                    token_type = "Bearer",
                    expires_in = 3600
                });
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected exception when ran Authenticate call with params: {nameof(client_id)}: {client_id}, {nameof(redirect_uri)}: {redirect_uri}, {nameof(grant_type)}: {grant_type}, {nameof(code)}: {code}, {nameof(code_verifier)}: {code_verifier}", ex);
                return RedirectToError(redirect_uri, "server_error");
            }
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
                default:
                    _logger.LogError($"Unexpected OAuth error response with params: {nameof(client_id)}: {client_id}, {nameof(code)}: {code}, {nameof(code_verifier)}: {code_verifier}");
                    return GenerateBadRequest("server_error");

            }
        }

        private async Task<IActionResult?> ValidateAuthRequestParameters(string redirect_uri, string client_id, string state, string response_type, string scope, string code_challenge, string code_challenge_method)
        {
            var clientIdRedirectUrlErrorResponse = await ValidateRedirectUriAndClientId(redirect_uri, client_id, true, state);
            if (clientIdRedirectUrlErrorResponse != null)
            {
                return clientIdRedirectUrlErrorResponse;
            }
            if (string.IsNullOrWhiteSpace(response_type)) return RedirectToError(redirect_uri, "unsupported_response_type", state);
            if (string.IsNullOrWhiteSpace(scope)) return RedirectToError(redirect_uri, "invalid_scope", state); // HACK: VALIDATE SCOPE
            if (string.IsNullOrWhiteSpace(state)) return RedirectToError(redirect_uri, "invalid_request", state);
            if (string.IsNullOrWhiteSpace(code_challenge)) return RedirectToError(redirect_uri, "invalid_request", state); // HACK: VALIDATE CODE_CHALLENGE FOR PKCE
            if (string.IsNullOrWhiteSpace(code_challenge_method)) return RedirectToError(redirect_uri, "invalid_request", state); // // HACK: VALIDATE CODE_CHALLENGE_METHOD FOR PKCE
            if (!response_type.Equals("code")) return RedirectToError(redirect_uri, "invalid_request", state);

            // all good
            return null;
        }
        private async Task<IActionResult?> ValidateRedirectUriAndClientId(
            string redirect_uri, string client_id, bool redirectsToClientApp = true ,string state = "")
        {
            var redirectUriError = await ValidateRedirectUri(redirect_uri);
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
                        if (redirectsToClientApp) return RedirectToError(redirect_uri, errorResponse, state);
                        else return GenerateBadRequest(errorResponse);
                    default:
                        _logger.LogError($"Unexpected OAuth error response with params: {nameof(client_id)}: {client_id}, {nameof(redirect_uri)}: {redirect_uri}", redirectUriError.Value);
                        errorResponse = "server_error";
                        if (redirectsToClientApp) return RedirectToError(redirect_uri, errorResponse, state);
                        else return GenerateBadRequest(errorResponse);
                }
            }
            var clientIdError = await ValidateClientId(client_id);
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
        private async Task<OAuthErrorResponse?> ValidateClientId(string inputClientId)
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
        private async Task<OAuthErrorResponse?> ValidateRedirectUri(string inputRedirectUri)
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
