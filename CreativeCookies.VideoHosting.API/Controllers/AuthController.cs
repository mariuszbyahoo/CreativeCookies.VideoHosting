using CreativeCookies.VideoHosting.Contracts.DTOs.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> Authenticate([FromQuery]string client_id, [FromQuery] string redirect_uri, [FromQuery] string response_type, [FromQuery] string scope, [FromQuery] string state)
        {
            if(string.IsNullOrWhiteSpace(client_id)) return BadRequest("No empty client_id");
            if(string.IsNullOrWhiteSpace(redirect_uri)) return BadRequest("No empty redirect_uri");
            if(string.IsNullOrWhiteSpace(response_type)) return BadRequest("No empty response_type");
            if(string.IsNullOrWhiteSpace(scope)) return BadRequest("No empty scope");
            if(string.IsNullOrWhiteSpace(state)) return BadRequest("No empty state");

            if(!response_type.Equals("code")) return BadRequest("Check response_type");

            var lookup = await _store.FindByClientIdAsync(client_id);

            if (lookup == null) return NotFound("Client not found");

            if (!redirect_uri.Equals(lookup.RedirectUri)) return BadRequest("Check redirect_uri");

            if(!User.Identity.IsAuthenticated)
            {
                // Redirect to login view
            }

            // Optional - display a screen to get user's permissions (if necessary)


        }
    }
}
