// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe;
using CreativeCookies.VideoHosting.Contracts.Services.IdP;
using CreativeCookies.VideoHosting.Contracts.Services.OAuth;
using CreativeCookies.VideoHosting.Infrastructure.Azure.Wrappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Stripe;

namespace CreativeCookies.VideoHosting.API.Areas.Identity.Pages.Account.Manage
{
    public class DeletePersonalDataModel : PageModel
    {
        private readonly IMyHubUserManager _userManager;
        private readonly IMyHubSignInManager _signInManager;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IConnectAccountsService _connectAccountsService;
        private readonly string _stripeApiSecretKey;
        private readonly ILogger<DeletePersonalDataModel> _logger;


        public DeletePersonalDataModel(
            IMyHubUserManager userManager,
            IMyHubSignInManager signInManager,
            IRefreshTokenService refreshTokenService,
            IConnectAccountsService connectAccountsService,
            StripeSecretKeyWrapper wrapper,
            ILogger<DeletePersonalDataModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _refreshTokenService = refreshTokenService;
            _connectAccountsService = connectAccountsService;
            _stripeApiSecretKey = wrapper.Value;
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public bool RequirePassword { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // HACK: Add deletion of underlying Stripe entities: Subscription, consumer and so on.
            var user = await _userManager.GetUserAsync(User);
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
            };
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            // HACK: Should I also send a refund?
            RequirePassword = await _userManager.HasPasswordAsync(user);
            if (RequirePassword)
            {
                if (!await _userManager.CheckPasswordAsync(user, Input.Password))
                {
                    ModelState.AddModelError(string.Empty, "Incorrect password.");
                    return Page();
                }
            }
            await DeleteStripeEntities(user.StripeCustomerId);

            var userId = await _userManager.GetUserIdAsync(user);
            await _refreshTokenService.DeleteIssuedRefreshTokens(Guid.Parse(userId));

            Response.Cookies.Delete("stac", cookieOptions);
            Response.Cookies.Delete("ltrt", cookieOptions);

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleting user.");
            }

            await _signInManager.SignOutAsync();

            _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

            return Redirect("~/");
        }

        private async Task DeleteStripeEntities(string stripeCustomerId)
        {
            StripeConfiguration.ApiKey = _stripeApiSecretKey;

            var subscriptionService = new SubscriptionService();
            var subscriptionListOptions = new SubscriptionListOptions
            {
                Customer = stripeCustomerId
            };
            var requestOptions = await GetRequestOptions();
            StripeList<Subscription> subscriptions = subscriptionService.List(subscriptionListOptions, requestOptions);

            foreach (var subscription in subscriptions)
            {
                string subscriptionId = subscription.Id;

                var paymentService = new PaymentIntentService();
                var paymentIntents = paymentService.List(
                    new PaymentIntentListOptions
                    {
                        Customer = stripeCustomerId,
                    }, requestOptions
                ).ToList();

                if (paymentIntents.Count > 0)
                {
                    foreach (var paymentIntent in paymentIntents)
                    {
                        var refundService = new RefundService();
                        var refundOptions = new RefundCreateOptions
                        {
                            PaymentIntent = paymentIntent.Id
                        };
                        refundService.Create(refundOptions, requestOptions);
                    }
                }

                var subscriptionCancelOptions = new SubscriptionCancelOptions()
                {
                    Prorate = true
                };
                try
                {
                    subscriptionService.Cancel(subscriptionId, subscriptionCancelOptions, requestOptions);
                }
                catch (StripeException ex)
                {
                    if (!ex.StripeError.Code.Equals("charge_already_refunded"))
                        _logger.LogError(ex, ex.Message);
                }
                // Delete customer
                var customerService = new CustomerService();
                customerService.Delete(stripeCustomerId, requestOptions: requestOptions);
            }
        }

        private async Task<RequestOptions?> GetRequestOptions()
        {
            var accountId = string.Empty;

            accountId = _connectAccountsService.GetConnectedAccountId();
            var requestOptions = new RequestOptions();
            requestOptions.StripeAccount = accountId;
            if (string.IsNullOrWhiteSpace(accountId)) return null;
            return requestOptions;
        }
    }
}
