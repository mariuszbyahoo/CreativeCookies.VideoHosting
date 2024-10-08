// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using CreativeCookies.VideoHosting.API.Helpers;
using CreativeCookies.VideoHosting.Contracts.Email;
using CreativeCookies.VideoHosting.Contracts.Services.IdP;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;

namespace CreativeCookies.VideoHosting.API.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly IMyHubUserManager _userManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ForgotPasswordModel> _logger;
        private readonly IStringLocalizer<ForgotPasswordModel> _stringLocalizer;

        public ForgotPasswordModel(IMyHubUserManager userManager, IEmailService emailService, IConfiguration configuration, ILogger<ForgotPasswordModel> logger, IStringLocalizer<ForgotPasswordModel> stringLocalizer)
        {
            _userManager = userManager;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
            _stringLocalizer = stringLocalizer;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                await _emailService.SendResetPasswordLinkAsync(
                    Input.Email,
                    _stringLocalizer["ForgotPasswordEmailSubject"],
                    _stringLocalizer["ForgotPasswordEmailTxt"], _configuration.GetValue<string>("WebsiteName"), callbackUrl);

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
