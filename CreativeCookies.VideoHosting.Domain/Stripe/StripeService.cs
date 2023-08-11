using CreativeCookies.VideoHosting.Contracts.Enums;
using CreativeCookies.VideoHosting.Contracts.Stripe;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Domain.Stripe
{
    public class StripeService : IStripeService
    {
        private readonly IConfiguration _configuration;
        private readonly string _stripeSecretAPIKey;
        private readonly ILogger<ConnectAccountsRepository> _logger;
        public StripeService(IConfiguration configuration, ILogger<ConnectAccountsRepository> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _stripeSecretAPIKey = _configuration.GetValue<string>("StripeSecretAPIKey");
        }
        public StripeConnectAccountStatus GetAccountStatus(string idStoredInDatabase)
        {
            StripeConfiguration.ApiKey = _stripeSecretAPIKey;
            var result = StripeConnectAccountStatus.Disconnected;

            var service = new AccountService();
            Account stripeAccount = service.Get(idStoredInDatabase); // HACK: this throws an exception if no account found

            if (stripeAccount != null)
            {
                if (stripeAccount.Capabilities != null &&
                   stripeAccount.Capabilities.CardPayments != null && stripeAccount.Capabilities.CardPayments.Equals("active") &&
                   stripeAccount.Capabilities.Transfers != null && stripeAccount.Capabilities.Transfers.Equals("active"))
                {
                    if (stripeAccount.Requirements != null &&
                       (stripeAccount.Requirements.PastDue == null || !stripeAccount.Requirements.PastDue.Any()))
                    {
                        result = StripeConnectAccountStatus.Connected;
                    }
                    else
                    {
                        result = StripeConnectAccountStatus.Restricted;
                    }
                }
                else if (stripeAccount.Capabilities != null && !stripeAccount.DetailsSubmitted)
                {
                    result = StripeConnectAccountStatus.Restricted;
                }
            }
            return result;
        }


        public async Task<string> GetConnectAccountLink()
        {
            try
            {
                StripeConfiguration.ApiKey = _stripeSecretAPIKey;
                var apiUrl = _configuration.GetValue<string>("ApiUrl");
                var clientUrl = _configuration.GetValue<string>("ClientUrl");

                // Check if an account already exists
                var accountOptions = new AccountCreateOptions { Type = "standard" };
                var accountSrv = new AccountService();
                var account = accountSrv.Create(accountOptions); // This throws a StripeException if not found - use StripeResult<T>
                // await SaveConnectedAccount(account.Id); HACK TODO: This has to be handled elswhere as this belongs to ConnectAccountsRepository

                var linkOptions = new AccountLinkCreateOptions
                {
                    Account = account.Id,
                    RefreshUrl = $"{apiUrl}/api/Stripe/OnboardingRefresh",
                    ReturnUrl = $"{clientUrl}/stripeOnboardingReturn",
                    Type = "account_onboarding",
                };
                var accountLinkSrv = new AccountLinkService();
                var link = accountLinkSrv.Create(linkOptions);
                return link.Url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in ReturnConnectAccountLink()");
                return string.Empty;
            }
        }
    }
}
