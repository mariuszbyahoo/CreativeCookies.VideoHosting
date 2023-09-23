using CreativeCookies.VideoHosting.Contracts.Services;
using CreativeCookies.VideoHosting.DTOs.OAuth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Crypto.Parameters;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CreativeCookies.VideoHosting.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUsersService _srv;

        public UsersController(IUsersService usersService)
        {
            _srv = usersService;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin,ADMIN")]
        public async Task<ActionResult<IList<MyHubUserDto>>> GetAll(int pageNumber = 1, int pageSize = 10, string search = "", string role = "any")
        { 
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest("PageNumber and PageSize must be greater than zero.");
            }

            var result = await _srv.GetUsersPaginatedResult(search, pageNumber, pageSize, role);

            return Ok(result);
        }

        [HttpGet("IsUserSubscriber")]
        [Authorize]
        public async Task<ActionResult<bool>> IsUserASubscriber()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var stac = Request.Cookies["stac"];
            if (tokenHandler.CanReadToken(stac))
            {
                var token = tokenHandler.ReadJwtToken(stac);
                string userId = null;

                foreach (var claim in token.Claims)
                {
                    if (claim.Type.Equals("nameid"))
                    {
                        userId = claim.Value;
                        break;
                    }
                }

                if (userId != null)
                {
                    return await _srv.IsUserSubscriber(userId);
                }
            }
            return false;
        }
    }
}
