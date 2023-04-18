using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace CreativeCookies.VideoHosting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;    
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody]LoginInput input)
        {
            var result = await _signInManager.PasswordSignInAsync(input.Login, input.Password, input.IsPersistent, false);

            if (result.Succeeded)
            {
                return Ok($"User with login: {input.Login} has logged in!");
            }
            else return BadRequest($"Errors: succeeded: {result.Succeeded}, isLockedOut: {result.IsLockedOut}, isNotAllowed:{result.IsNotAllowed}, requiresTwoFactor: {result.RequiresTwoFactor}");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]RegisterInput input)
        {
            var newUser = new IdentityUser { UserName = input.Login };
            var result = await _userManager.CreateAsync(newUser, input.Password);
            if (result.Succeeded)
            {
                return Ok($"Succesfully registered user: {System.Text.Json.JsonSerializer.Serialize(result)}, {System.Text.Json.JsonSerializer.Serialize(input)}");
            }
            else return BadRequest($"Something went wrong, {result.Errors}");
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userName = _signInManager.Context.User.Claims.FirstOrDefault(c => c.Value == "jasio");
            await _signInManager.SignOutAsync();
            return Ok($"Logged out user: {userName}");
        }
    }
    public class LoginInput
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public bool IsPersistent { get; set; }
    }

    public class RegisterInput
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
