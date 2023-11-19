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
using CreativeCookies.VideoHosting.DTOs.OAuth;
using CreativeCookies.VideoHosting.Contracts.Email;
using CreativeCookies.VideoHosting.API.Attributes;

namespace CreativeCookies.VideoHosting.API.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IMyHubSignInManager _signInManager;
        private readonly IMyHubUserManager _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public RegisterModel(
            IMyHubUserManager userManager,
            IMyHubSignInManager signInManager,
            ILogger<RegisterModel> logger,
            IConfiguration configuration,
            IEmailService emailService)
        {
            _userManager = userManager;
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
            [Required(ErrorMessage = "Email jest wymagany")]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Hasło jest wymagane")]
            [StringLength(100, ErrorMessage = "{0} musi być dłuższe niż {2} oraz krótsze niż {1}", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "Hasła są różne")]
            public string ConfirmPassword { get; set; }

            [MustBeTrue(ErrorMessage = "Musisz zaakceptować Regulamin Serwisu i Politykę Prywatności.")]
            [Display(Name = "Oświadczam, że zapoznałam/em się z Regulaminem Serwisu i Polityką Prywatności Serwisu oraz akceptuję ich treść.")]
            public bool AcceptTermsAndPrivacy { get; set; }

            [MustBeTrue(ErrorMessage = "Musisz wyrazić zgodę na przetwarzanie danych osobowych.")]
            [Display(Name = "Wyrażam zgodę na przetwarzanie moich danych osobowych w celach marketingowych przez Operatora Serwisu. Moja zgoda obejmuje nazwę lub imię i nazwisko oraz adres e-mail podane podczas dokonania Rejestracji.")]
            public bool ConsentToProcessData { get; set; }

            [MustBeTrue(ErrorMessage = "Musisz wyrazić zgodę na używanie środków komunikacji elektronicznej.")]
            [Display(Name = "Wyrażam zgodę na używanie przez Operatora Serwisu środków komunikacji elektronicznej oraz telekomunikacyjnych urządzeń końcowych w celu przesyłania mi informacji handlowych oraz prowadzenia marketingu przez Operatora Serwisu.")]
            public bool ConsentToElectronicCommunication { get; set; }
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
                var user = new MyHubUserDto(Guid.Empty, Input.Email, string.Empty, false, string.Empty, DateTime.MinValue, DateTime.MinValue, null); 

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
    }
}
