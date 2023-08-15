using CreativeCookies.VideoHosting.Contracts.Enums;
using CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe;
using CreativeCookies.VideoHosting.DTOs.Stripe;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stripe;

namespace CreativeCookies.VideoHosting.Infrastructure.Stripe
{
    public class StripeOnboardingService : IStripeOnboardingService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripeOnboardingService> _logger;
        private readonly string _stripeSecretAPIKey;
        private readonly string _apiUrl;
        private readonly string _clientUrl;

        public StripeOnboardingService(IConfiguration configuration, ILogger<StripeOnboardingService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _stripeSecretAPIKey = _configuration.GetValue<string>("StripeSecretAPIKey");
            _apiUrl = _configuration.GetValue<string>("ApiUrl");
            _clientUrl = _configuration.GetValue<string>("ClientUrl");
        }

        public StripeResultDto<StripeConnectAccountStatus> GetAccountStatus(string idStoredInDatabase)
        {
            StripeConfiguration.ApiKey = _stripeSecretAPIKey;
            var status = StripeConnectAccountStatus.Disconnected;
            var stripeAccountResponse = GetStripeAccount(idStoredInDatabase);
            if (!string.IsNullOrWhiteSpace(stripeAccountResponse.ErrorMessage))
            {
                _logger.LogError(stripeAccountResponse.ErrorMessage);
                return new StripeResultDto<StripeConnectAccountStatus>(true, status, stripeAccountResponse.ErrorMessage);
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
            var result = new StripeResultDto<StripeConnectAccountStatus>(true, status, string.Empty);
            _logger.LogInformation($"IStripeService.GetAccountStatus(string idStoredInDatabase) called, returned: {JsonConvert.SerializeObject(result)}");
            return result;
        }

        public StripeResultDto<AccountCreationResultDto> GenerateConnectAccountLink()
        {
            try
            {
                StripeConfiguration.ApiKey = _stripeSecretAPIKey;

                var accountOptions = new AccountCreateOptions { Type = "standard" };
                var accountSrv = new AccountService();
                var accountResponse = CreateStripeAccount(accountOptions);
                if (!accountResponse.Success)
                {
                    _logger.LogError(accountResponse.ErrorMessage);
                    return new StripeResultDto<AccountCreationResultDto>(false, null, accountResponse.ErrorMessage);
                }

                var account = accountResponse.Data;

                var link = GenerateLink(account.Id);
                var result = new StripeResultDto<AccountCreationResultDto>(true, new AccountCreationResultDto(link.Url, accountResponse.Data.Id), string.Empty);
                _logger.LogInformation($"IStripeService.GenerateConnectAccountLink() called, returned: {JsonConvert.SerializeObject(result)}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in ReturnConnectAccountLink()");
                return new StripeResultDto<AccountCreationResultDto>(false, null, $"Exception occured: {ex.Message}, ex.InnerException: {ex.InnerException}, ex.StackTrace: {ex.StackTrace}, ex.Source: {ex.Source} \nCreativeCookies.VideoHosting.Domain.Stripe.StripeService line 97");
            };
        }

        public StripeResultDto<AccountCreationResultDto> GenerateConnectAccountLink(string existingAccountId)
        {
            try
            {
                StripeConfiguration.ApiKey = _stripeSecretAPIKey;

                var link = GenerateLink(existingAccountId);
                var result = new StripeResultDto<AccountCreationResultDto>(true, new AccountCreationResultDto(link.Url, existingAccountId), string.Empty);
                _logger.LogInformation($"IStripeService.GenerateConnectAccountLink(string existingAccountId) called, returned: {JsonConvert.SerializeObject(result)}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in ReturnConnectAccountLink()");
                return new StripeResultDto<AccountCreationResultDto>(false, null, $"Exception occured: {ex.Message}, ex.InnerException: {ex.InnerException}, ex.StackTrace: {ex.StackTrace}, ex.Source: {ex.Source} \nCreativeCookies.VideoHosting.Domain.Stripe.StripeService line 97");
            }
        }

        #region private methods

        /// <summary>
        /// Generates a connect account's onboarding link for an account
        /// </summary>
        /// <param name="accountId">accountId</param>
        /// <returns>Object from Stripe SDK</returns>
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

        /// <summary>
        /// Gets a Stripe account, or an object with null as Data if none found in Stripe's API
        /// </summary>
        /// <param name="accountId">Id of an account you're looking for</param>
        /// <returns>
        /// IStripeResult with an account as data and Success = true if everything was ok or,
        /// IStripeResult with an Success = false and an ErrorMessage = StripeException.message if none found
        /// </returns>
        private StripeResultDto<Account> GetStripeAccount(string accountId)
        {
            try
            {
                var service = new AccountService();
                var account = service.Get(accountId);
                return new StripeResultDto<Account>(true, account, string.Empty);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, $"Error fetching account with ID {accountId}");
                return new StripeResultDto<Account>(false, null, ex.Message);
            }
        }

        /// <summary>
        /// Creates an account using Stripe API and then returns it within a StripeResult.
        /// If StripeException occurs, then returns a StripeResult with Success = false and
        /// an ex.Message in an ErrorMessage field
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private StripeResultDto<Account> CreateStripeAccount(AccountCreateOptions options)
        {
            try
            {
                var service = new AccountService();
                var account = service.Create(options);
                return new StripeResultDto<Account>(true, account, string.Empty);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error creating Stripe account");
                return new StripeResultDto<Account>(false, null, ex.Message);
            }
        }
        #endregion
    }
}
