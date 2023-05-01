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

        public AuthController(IClientStore store, IAuthorizationCodeRepository codesRepo, ILogger<AuthController> logger, IConfiguration configuration)
        {
            _store = store;
            _codesRepo = codesRepo;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet("authorize")]
        public async Task<IActionResult> Authorize([FromQuery]string? client_id, [FromQuery] string? redirect_uri, 
            [FromQuery] string? response_type, [FromQuery] string? scope, [FromQuery] string? state,
            [FromQuery] string? code_challenge, [FromQuery] string? code_challenge_method)
        {
            try
            {
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
                var clientIdRedirectUrlErrorResponse = await ValidateRedirectUriAndClientId(redirect_uri, client_id);
                if (clientIdRedirectUrlErrorResponse != null)
                {
                    return clientIdRedirectUrlErrorResponse;
                }
                var codeAndCodeVerifierErrorResponse = await ValidateCodeAndCodeVerifier(code, code_verifier, client_id, redirect_uri);
                if (codeAndCodeVerifierErrorResponse != null)
                {
                    return codeAndCodeVerifierErrorResponse;
                }
                if (!string.IsNullOrWhiteSpace(grant_type) && grant_type.Equals("authorization_code"))
                {
                    return RedirectToError(redirect_uri, "unsupported_grant_type");
                }

                var jwtSecretKey = _configuration["JWTSecretKey"];

                
                // If valid, generate an access token and a refresh token

                // Return the access token and refresh token as a JSON object
                return Ok(new
                {
                    access_token = "your_access_token",
                    token_type = "Bearer",
                    expires_in = 3600, // One hour
                    refresh_token = "your_refresh_token"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected exception when ran Authenticate call with params: {nameof(client_id)}: {client_id}, {nameof(redirect_uri)}: {redirect_uri}, {nameof(grant_type)}: {grant_type}, {nameof(code)}: {code}, {nameof(code_verifier)}: {code_verifier}", ex);
                return RedirectToError(redirect_uri, "server_error");
            }
        }

        private async Task<IActionResult?> ValidateCodeAndCodeVerifier(string code, string code_verifier, string client_id, string redirect_uri)
        {
            if(await _store.IsCodeValid(code, client_id))
            {
                return null;
            }
            return RedirectToError(redirect_uri, "invalid_request");
        }

        private async Task<IActionResult?> ValidateAuthRequestParameters(string redirect_uri, string client_id, string state, string response_type, string scope, string code_challenge, string code_challenge_method)
        {
            var clientIdRedirectUrlErrorResponse = await ValidateRedirectUriAndClientId(redirect_uri, client_id, state);
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
            string redirect_uri, string client_id, string state = "")
        {
            var redirectUriError = await ValidateRedirectUri(redirect_uri);
            if (redirectUriError != null && redirectUriError.HasValue)
            {
                var errorResponse = string.Empty;
                switch (redirectUriError.Value)
                {
                    case OAuthErrorResponses.InvalidRedirectUri:
                        _logger.LogDebug($"Received invalid request with an invalid redirect_uri, and params: {nameof(client_id)}: {client_id}, {nameof(redirect_uri)}: {redirect_uri}");
                        return BadRequest("Invalid redirect_uri");
                    case OAuthErrorResponses.InvalidRequest:
                        _logger.LogDebug($"Received invalid request with params: {nameof(client_id)}: {client_id}, {nameof(redirect_uri)}: {redirect_uri}");
                        errorResponse = "invalid_request";
                        return RedirectToError(redirect_uri, errorResponse, state);
                    default:
                        _logger.LogError($"Unexpected OAuth error response with params: {nameof(client_id)}: {client_id}, {nameof(redirect_uri)}: {redirect_uri}", redirectUriError.Value);
                        errorResponse = "server_error";
                        return RedirectToError(redirect_uri, errorResponse, state);
                }
            }
            var clientIdError = await ValidateClientId(client_id);
            if (clientIdError != null && clientIdError.HasValue)
            {
                var errorResponse = string.Empty;
                switch (clientIdError.Value)
                {
                    case OAuthErrorResponses.InvalidRequest:
                        _logger.LogDebug($"Received invalid request with params: {nameof(client_id)}: {client_id}, {nameof(redirect_uri)}: {redirect_uri}");
                        errorResponse = "invalid_request";
                        return RedirectToError(redirect_uri, errorResponse, state);
                    case OAuthErrorResponses.UnauthorisedClient:
                        _logger.LogDebug($"Received invalid request with an unauthorised client, and params: {nameof(client_id)}: {client_id}, {nameof(redirect_uri)}: {redirect_uri}");
                        errorResponse = "unauthorised_client";
                        return RedirectToError(redirect_uri, errorResponse, state);
                    default:
                        _logger.LogError($"Unexpected OAuth error response with params: {nameof(client_id)}: {client_id}, {nameof(redirect_uri)}: {redirect_uri}", clientIdError.Value);
                        errorResponse = "server_error";
                        return RedirectToError(redirect_uri, errorResponse, state);
                }
            }

            if (await _store.WasRedirectUriRegisteredToClient(redirect_uri, client_id))
            {
                // all good, no errors to return 
                return null;
            }
            else return BadRequest("Invalid redirect_uri");
        }
        private async Task<OAuthErrorResponses?> ValidateClientId(string inputClientId)
        {
            Guid clientId;
            var parsedSuccessfully = Guid.TryParse(inputClientId, out clientId);
            if (string.IsNullOrWhiteSpace(inputClientId) || !parsedSuccessfully)
            {
                return OAuthErrorResponses.InvalidRequest;
            }
            if (parsedSuccessfully && Guid.Empty != clientId)
            {
                var lookup = await _store.FindByClientIdAsync(clientId);
                if (lookup == null) return OAuthErrorResponses.UnauthorisedClient;
            }
            return null;
        }
        private async Task<OAuthErrorResponses?> ValidateRedirectUri(string inputRedirectUri)
        {
            if (!string.IsNullOrWhiteSpace(inputRedirectUri) && await _store.IsRedirectUriPresentInDatabase(inputRedirectUri))
            {
                return null;
            }

            return OAuthErrorResponses.InvalidRedirectUri;
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

    }
}
