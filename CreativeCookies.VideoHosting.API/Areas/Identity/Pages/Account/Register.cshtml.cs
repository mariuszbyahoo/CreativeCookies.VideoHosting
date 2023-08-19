// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using CreativeCookies.VideoHosting.API.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using CreativeCookies.VideoHosting.Contracts.Services.IdP;
using CreativeCookies.VideoHosting.DAL.DAOs.OAuth;
using CreativeCookies.VideoHosting.DTOs.OAuth;

namespace CreativeCookies.VideoHosting.API.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IMyHubSignInManager _signInManager;
        private readonly IMyHubUserManager _userManager;
        private readonly IMyHubUserStore _userStore; 
        private readonly IUserEmailStore<IdentityUser> _emailStore; // HACK TODO Wrapper
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public RegisterModel(
            IMyHubUserManager userManager,
            IMyHubUserStore userStore,
            IMyHubSignInManager signInManager,
            ILogger<RegisterModel> logger,
            IConfiguration configuration,
            IEmailService emailService)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _configuration = configuration;
            _emailService = emailService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new MyHubUserDto(Guid.Empty, "", "", false, ""); // HACK TODO

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, "NonSubscriber");
                    if (roleResult.Succeeded)
                    {
                        _logger.LogInformation("User created a new account with password.");
                    }
                    else _logger.LogError($"Error occured while adding user: {user.Id} to role: NonSubscriber");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    var websiteName = _configuration.GetValue<string>("WebsiteName");
                    var websiteUrl = _configuration.GetValue<string>("ClientUrl");
                    var wasEmailSent = await _emailService.SendAccountActivationEmailAsync(
                        Input.Email, 
                        $"Confirm your account at {websiteName}", 
                        $"You're recieving this email because you've requested to sign in at {websiteName}: {websiteUrl}",
                        websiteUrl, websiteName, callbackUrl);
                    if (wasEmailSent)
                    {
                        return LocalRedirect("~/Identity/Account/ConfirmAccount");
                    }
                    else
                    {
                        return LocalRedirect("~/Identity/Account/EmailWasNotSentDueToError");
                        // HACK: TODO do some kind of transaction cancelling (just delete previousely created account because of the error).
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private MyHubUserDto CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.GetManagerSupportsUserEmail())
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
