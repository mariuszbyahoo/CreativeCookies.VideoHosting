// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using CreativeCookies.VideoHosting.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using CreativeCookies.VideoHosting.Contracts.Services.IdP;
using CreativeCookies.VideoHosting.Contracts.Email;

namespace CreativeCookies.VideoHosting.API.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly IMyHubUserManager _userManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public ResendEmailConfirmationModel(IMyHubUserManager userManager, IEmailService emailService, IConfiguration configuration)
        {
            _userManager = userManager;
            _emailService = emailService;
            _configuration = configuration;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = userId, code = code },
                protocol: Request.Scheme);
            var websiteName = _configuration.GetValue<string>("WebsiteName");
            var websiteUrl = _configuration.GetValue<string>("ClientUrl");
            var wasEmailSent = await _emailService.SendAccountActivationEmailAsync(
                Input.Email,
                $"Potwierdź swoje konto na {websiteName}",
                $"Otrzymałeś tego maila, ponieważ zarejestrowałeś nowe konto na {websiteName}: {websiteUrl}",
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
    }
}
