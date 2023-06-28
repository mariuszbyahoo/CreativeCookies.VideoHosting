using CreativeCookies.VideoHosting.Contracts.DTOs.OAuth;
using CreativeCookies.VideoHosting.Domain.DTOs.OAuth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace CreativeCookies.VideoHosting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;

        public UsersController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin,ADMIN")]
        public async Task<ActionResult<IList<IMyHubUser>>> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest("PageNumber and PageSize must be greater than zero.");
            }

            var users = _userManager.Users
                .OrderBy(x => x.Email)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new List<IMyHubUser>();

            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                result.Add(new MyHubUserDto(Guid.Parse(user.Id.ToUpperInvariant()), user.Email ?? user.UserName, string.Join(',', userRoles)));
            }

            return result;
        }
    }
}
