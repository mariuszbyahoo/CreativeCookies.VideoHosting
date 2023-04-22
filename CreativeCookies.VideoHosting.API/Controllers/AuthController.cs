using CreativeCookies.VideoHosting.Contracts.DTOs.OAuth;
using CreativeCookies.VideoHosting.DAL.DAOs.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Web;

namespace CreativeCookies.VideoHosting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IClientStore _store;

        public AuthController(IClientStore store)
        {
            _store = store;
        }

        [HttpGet]
        public async Task<IActionResult> Authenticate([FromQuery]string client_id, [FromQuery] string redirect_uri, 
            [FromQuery] string response_type, [FromQuery] string scope, [FromQuery] string state,
            [FromQuery] string code_challenge, [FromQuery] string code_challenge_method)
        {
            if(string.IsNullOrWhiteSpace(client_id)) return BadRequest("No empty client_id");
            if(string.IsNullOrWhiteSpace(redirect_uri)) return BadRequest("No empty redirect_uri");
            if(string.IsNullOrWhiteSpace(response_type)) return BadRequest("No empty response_type");
            if(string.IsNullOrWhiteSpace(scope)) return BadRequest("No empty scope");
            if(string.IsNullOrWhiteSpace(state)) return BadRequest("No empty state");
            if(string.IsNullOrWhiteSpace(code_challenge)) return BadRequest("No empty code_challenge");
            if(string.IsNullOrWhiteSpace(code_challenge_method)) return BadRequest("No empty code_challenge_method");

            if(!response_type.Equals("code")) return BadRequest("Invalid response_type");

            var lookup = await _store.FindByClientIdAsync(client_id);

            if (lookup == null) return NotFound("Client not registered");

            if (!redirect_uri.Equals(lookup.RedirectUri)) return BadRequest("Invalid redirect_uri");

            if(!User.Identity.IsAuthenticated)
            {
                throw new NotImplementedException("Add a login page!");
            }

            // Optional - display a screen to get user's permissions (if necessary)

            var authorizationCode = await _store.GetAuthorizationCode(client_id, User.FindFirstValue(ClaimTypes.NameIdentifier), redirect_uri, code_challenge, code_challenge_method);

            var redirectUriBuilder = new UriBuilder(redirect_uri);
            var queryParameters = HttpUtility.ParseQueryString(redirectUriBuilder.Query);
            queryParameters["code"] = authorizationCode;
            queryParameters["state"] = state;
            redirectUriBuilder.Query = queryParameters.ToString();

            return Redirect(redirectUriBuilder.ToString());
        }
    }
}
