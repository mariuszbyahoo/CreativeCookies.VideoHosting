using CreativeCookies.VideoHosting.Contracts.DTOs.OAuth;
using CreativeCookies.VideoHosting.Contracts.Enums;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.DAL.DAOs.OAuth;
using CreativeCookies.VideoHosting.Domain.OAuth.DTOs;
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

        public AuthController(IClientStore store, IAuthorizationCodeRepository codesRepo, ILogger<AuthController> logger)
        {
            _store = store;
            _codesRepo = codesRepo;
            _logger = logger;
        }

        [HttpGet("authorize")]
        public async Task<IActionResult> Authorize([FromQuery]string client_id, [FromQuery] string redirect_uri, 
            [FromQuery] string response_type, [FromQuery] string scope, [FromQuery] string state,
            [FromQuery] string code_challenge, [FromQuery] string code_challenge_method)
        {
            var clientIdError = await ValidateClientId(client_id);
            if (clientIdError != null && clientIdError.HasValue)
            {
                var errorResponse = string.Empty;
                switch(clientIdError.Value)
                {
                    case OAuthErrorResponses.InvalidRequest:
                        _logger.LogDebug($"Received invalid request with params: {nameof(client_id)}: {client_id}, {nameof(redirect_uri)}: {redirect_uri}, {nameof(response_type)}: {response_type}, {nameof(scope)}: {scope}, {nameof(state)}: {state}, {nameof(code_challenge)}: {code_challenge}, {nameof(code_challenge_method)}: {code_challenge_method}");
                        errorResponse = "invalid_request";
                        throw new NotImplementedException("return invalid_request");
                    case OAuthErrorResponses.UnauthorisedClient:
                        _logger.LogDebug($"Received invalid request with an unauthorised client, and params: {nameof(client_id)}: {client_id}, {nameof(redirect_uri)}: {redirect_uri}, {nameof(response_type)}: {response_type}, {nameof(scope)}: {scope}, {nameof(state)}: {state}, {nameof(code_challenge)}: {code_challenge}, {nameof(code_challenge_method)}: {code_challenge_method}");
                        errorResponse = "unauthorised_client";
                        throw new NotImplementedException("return unauthorised_client");
                    default:
                        _logger.LogError("Unexpected OAuth error response: ", clientIdError.Value);
                        throw new NotImplementedException("return server_error");
                }
            }
            var redirectUriError = await ValidateRedirectUri(redirect_uri);
            if (redirectUriError != null && redirectUriError.HasValue)
            {
                var errorResponse = string.Empty;
                switch (redirectUriError.Value)
                {
                    case OAuthErrorResponses.InvalidRedirectUri:
                        _logger.LogDebug($"Received invalid request with an invalid redirect_uri, and params: {nameof(client_id)}: {client_id}, {nameof(redirect_uri)}: {redirect_uri}, {nameof(response_type)}: {response_type}, {nameof(scope)}: {scope}, {nameof(state)}: {state}, {nameof(code_challenge)}: {code_challenge}, {nameof(code_challenge_method)}: {code_challenge_method}");
                        return BadRequest("Invalid redirect_uri");
                    case OAuthErrorResponses.InvalidRequest:
                        _logger.LogDebug($"Received invalid request with params: {nameof(client_id)}: {client_id}, {nameof(redirect_uri)}: {redirect_uri}, {nameof(response_type)}: {response_type}, {nameof(scope)}: {scope}, {nameof(state)}: {state}, {nameof(code_challenge)}: {code_challenge}, {nameof(code_challenge_method)}: {code_challenge_method}");
                        errorResponse = "invalid_request";
                        throw new NotImplementedException("return invalid_request");
                    default:
                        _logger.LogError($"Unexpected OAuth error response: ", redirectUriError.Value);
                        throw new NotImplementedException("return server_error");
                }
            }
            if (string.IsNullOrWhiteSpace(response_type)) return BadRequest("No empty response_type");
            if(string.IsNullOrWhiteSpace(scope)) return BadRequest("No empty scope");
            if(string.IsNullOrWhiteSpace(state)) return BadRequest("No empty state");
            if(string.IsNullOrWhiteSpace(code_challenge)) return BadRequest("No empty code_challenge");
            if(string.IsNullOrWhiteSpace(code_challenge_method)) return BadRequest("No empty code_challenge_method");

            if(!response_type.Equals("code")) return BadRequest("Invalid response_type");

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
            if (string.IsNullOrWhiteSpace(inputRedirectUri))
            {
                return OAuthErrorResponses.InvalidRequest;
            }
            if (await _store.IsRedirectUriPresentInDatabase(inputRedirectUri))
            {
                return null;
            }

            return OAuthErrorResponses.InvalidRedirectUri;
        }
    }
}
