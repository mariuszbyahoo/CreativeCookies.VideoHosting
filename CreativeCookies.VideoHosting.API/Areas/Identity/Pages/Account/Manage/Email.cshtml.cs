// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using CreativeCookies.VideoHosting.API.Helpers;
using CreativeCookies.VideoHosting.Contracts.Services.IdP;
using CreativeCookies.VideoHosting.DTOs.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;

namespace CreativeCookies.VideoHosting.API.Areas.Identity.Pages.Account.Manage
{
    public class EmailModel : PageModel
    {
        private readonly IMyHubUserManager _userManager;
        private readonly IMyHubSignInManager _signInManager;
        private readonly IEmailService _emailSender;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailModel> _logger;
        private readonly IStringLocalizer<EmailModel> _stringLocalizer;
        public EmailModel(
            IMyHubUserManager userManager,
            IMyHubSignInManager signInManager,
            IEmailService emailSender,
            IConfiguration configuration,
            ILogger<EmailModel> logger,
            IStringLocalizer<EmailModel> stringLocalizer)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _configuration = configuration;
            _logger = logger;
            _stringLocalizer= stringLocalizer;
        }

        public string Email { get; set; }

        public bool IsEmailConfirmed { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }



        private async Task LoadAsync(MyHubUserDto user)
        {
            var email = await _userManager.GetEmailAsync(user);
            Email = email;

            Input = new InputModel
            {
                NewEmail = email,
            };

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostChangeEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var email = await _userManager.GetEmailAsync(user);
            if (Input.NewEmail != email)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var emailChangeLink = Url.Page(
                    "/Account/ConfirmEmailChange",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, email = Input.NewEmail, code = code },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailChangeLinkAsync(
                    Input.NewEmail,
                    _stringLocalizer["ConfirmEmail"],
                    _stringLocalizer["ConfirmEmailSubject"],
                    _configuration.GetValue<string>("WebsiteName"),
                    emailChangeLink
                    );

                StatusMessage = $"{_stringLocalizer["EmailChangeSent"]} {Input.NewEmail}";
                return RedirectToPage();
            }

            StatusMessage = _stringLocalizer["EmailIsUnchanged"];
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code },
                protocol: Request.Scheme);

            var websiteName = _configuration.GetValue<string>("WebsiteName");
            var websiteUrl = _configuration.GetValue<string>("ClientUrl");

            await _emailSender.SendAccountActivationEmailAsync(
                email,
                $"{_stringLocalizer["ConfirmYourAccountAt"]} {websiteName}",
                $"{_stringLocalizer["RegisterMailSubject"]} {websiteName}: {websiteUrl}",
                websiteUrl, websiteName, callbackUrl);

            StatusMessage = $"{_stringLocalizer["EmailChangeSent"]} {email}";
            return RedirectToPage();
        }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "New email")]
            public string NewEmail { get; set; }
        }
    }
}
