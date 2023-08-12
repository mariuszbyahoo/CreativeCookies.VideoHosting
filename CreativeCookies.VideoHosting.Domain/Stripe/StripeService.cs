using CreativeCookies.VideoHosting.Contracts.Enums;
using CreativeCookies.VideoHosting.Contracts.Stripe;
using CreativeCookies.VideoHosting.Contracts.Wrappers;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.Domain.Repositories;
using CreativeCookies.VideoHosting.Domain.Wrappers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;

namespace CreativeCookies.VideoHosting.Domain.Stripe
{
    public class StripeService : IStripeService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConnectAccountsRepository> _logger;
        private readonly string _stripeSecretAPIKey;
        private readonly string _apiUrl;
        private readonly string _clientUrl;
        
        public StripeService(IConfiguration configuration, ILogger<ConnectAccountsRepository> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _stripeSecretAPIKey = _configuration.GetValue<string>("StripeSecretAPIKey");
            _apiUrl = _configuration.GetValue<string>("ApiUrl");
            _clientUrl = _configuration.GetValue<string>("ClientUrl");
        }

        public IStripeResult<StripeConnectAccountStatus> GetAccountStatus(string idStoredInDatabase)
        {
            StripeConfiguration.ApiKey = _stripeSecretAPIKey;
            var status = StripeConnectAccountStatus.Disconnected;
            var stripeAccountResponse = GetStripeAccount(idStoredInDatabase);
            if (!stripeAccountResponse.Success)
            {
                _logger.LogError(stripeAccountResponse.ErrorMessage);
                return new StripeResult<StripeConnectAccountStatus>() 
                { 
                    Data = StripeConnectAccountStatus.Disconnected, Success = false, 
                    ErrorMessage = stripeAccountResponse.ErrorMessage 
                }; 
            }

            var stripeAccount = stripeAccountResponse.Data;

            if (stripeAccount != null)
            {
                if (stripeAccount.Capabilities != null &&
                   stripeAccount.Capabilities.CardPayments != null && stripeAccount.Capabilities.CardPayments.Equals("active") &&
                   stripeAccount.Capabilities.Transfers != null && stripeAccount.Capabilities.Transfers.Equals("active"))
                {
                    if (stripeAccount.Requirements != null &&
                       (stripeAccount.Requirements.PastDue == null || !stripeAccount.Requirements.PastDue.Any()))
                    {
                        status = StripeConnectAccountStatus.Connected;
                    }
                    else
                    {
                        status = StripeConnectAccountStatus.Restricted;
                    }
                }
                else if (stripeAccount.Capabilities != null && !stripeAccount.DetailsSubmitted)
                {
                    status = StripeConnectAccountStatus.Restricted;
                }
            }
            var result = new StripeResult<StripeConnectAccountStatus>()
            {
                Data = status,
                Success = true,
                ErrorMessage = string.Empty
            };
            return result;
        }

        public IStripeResult<string> GetConnectAccountLink()
        {
            try
            {
                StripeConfiguration.ApiKey = _stripeSecretAPIKey;

                // Check if an account already exists
                var accountOptions = new AccountCreateOptions { Type = "standard" };
                var accountSrv = new AccountService();
                var accountResponse = CreateStripeAccount(accountOptions);
                if (!accountResponse.Success)
                {
                    _logger.LogError(accountResponse.ErrorMessage);
                    return new StripeResult<string>() { Data = string.Empty, Success = false, ErrorMessage = accountResponse.ErrorMessage };
                }

                var account = accountResponse.Data;
                // await SaveConnectedAccount(account.Id); HACK TODO: This has to be handled elswhere as this belongs to ConnectAccountsRepository

                var link = GenerateLink(account.Id);
                if (link != null)
                    return new StripeResult<string>() { Data = link.Url, Success = true };
                else 
                    return new StripeResult<string>() { Data = string.Empty, Success = false, ErrorMessage = "A Stripe's AccountLinkService returned null instead of valid AccountLink." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in ReturnConnectAccountLink()");
                return new StripeResult<string>() { Data = string.Empty, Success = false, ErrorMessage = $"Unexpected Error occured: {ex.Message}, ex.InnerException: {ex.InnerException}, ex.StackTrace: {ex.StackTrace}, ex.Source: {ex.Source} \nCreativeCookies.VideoHosting.Domain.Stripe.StripeService line 97" };
            }
        }
        #region private methods
        private AccountLink GenerateLink(string accountId)
        {
            var linkOptions = new AccountLinkCreateOptions
            {
                Account = accountId,
                RefreshUrl = $"{_apiUrl}/api/Stripe/OnboardingRefresh",
                ReturnUrl = $"{_clientUrl}/stripeOnboardingReturn",
                Type = "account_onboarding",
            };
            var accountLinkSrv = new AccountLinkService();
            return accountLinkSrv.Create(linkOptions);
        }
        private StripeResult<Account> GetStripeAccount(string accountId)
        {
            try
            {
                var service = new AccountService();
                var account = service.Get(accountId);
                return new StripeResult<Account>
                {
                    Success = true,
                    Data = account
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, $"Error fetching account with ID {accountId}");
                return new StripeResult<Account>
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
        private StripeResult<Account> CreateStripeAccount(AccountCreateOptions options)
        {
            try
            {
                var service = new AccountService();
                var account = service.Create(options);
                return new StripeResult<Account>
                {
                    Success = true,
                    Data = account
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error creating Stripe account");
                return new StripeResult<Account>
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
        #endregion
    }
}
