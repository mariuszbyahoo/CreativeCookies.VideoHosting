using CreativeCookies.VideoHosting.Contracts.Repositories.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CreativeCookies.VideoHosting.API.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ILogger<LogoutModel> _logger;
        private readonly IConfiguration _configuration;

        public LogoutModel(SignInManager<IdentityUser> signInManager, ILogger<LogoutModel> logger, IConfiguration configuration, IRefreshTokenRepository refreshTokenRepository)
        {
            _signInManager = signInManager;
            _refreshTokenRepository = refreshTokenRepository;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Performs logout action
        /// </summary>
        /// <param name="returnPath">relative path : ex. "/films-list" param MUST contain slash at the beginning</param>
        /// <returns>Task<IActionResult></returns>
        private async Task<IActionResult> Logout(string returnPath = null)
        {
            await _signInManager.SignOutAsync();
            if (Request.Cookies.ContainsKey("ltrt"))
            {
                var refreshToken = Request.Cookies["ltrt"].ToString();
                await _refreshTokenRepository.RevokeRefreshToken(refreshToken);
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                };
                Response.Cookies.Delete("ltrt", cookieOptions);
            }
            if (Request.Cookies.ContainsKey("stac"))
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                };
                Response.Cookies.Delete("stac", cookieOptions);
            }

            _logger.LogInformation("User logged out.");
            var clientUrl = _configuration["ClientUrl"];

            if (!string.IsNullOrWhiteSpace(returnPath))
            {
                return Redirect($"{clientUrl}{returnPath}");
            }
            else
            {
                return Redirect($"{clientUrl}/films-list");
            }
        }

        public async Task<IActionResult> OnGetAsync(string returnPath = null)
        {
            return await Logout(returnPath);
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            return await Logout(returnUrl);
        }
    }

}
