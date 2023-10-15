using CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe;
using CreativeCookies.VideoHosting.Contracts.Services;
using CreativeCookies.VideoHosting.Infrastructure.Azure.Wrappers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using System.ComponentModel.DataAnnotations;

namespace CreativeCookies.VideoHosting.Infrastructure.Stripe
{
    public class StripeCustomerService : IStripeCustomerService
    {
        private readonly IUsersService _usersSrv;
        private readonly IConnectAccountsService _connectAccountsService;
        private readonly ILogger<StripeCustomerService> _logger;
        private readonly string _stripeSecretAPIKey;

        public StripeCustomerService(IUsersService usersSrv, ILogger<StripeCustomerService> logger, IConnectAccountsService connectAccountsService,
            StripeSecretKeyWrapper wrapper)
        {
            _usersSrv = usersSrv;
            _logger = logger;
            _connectAccountsService = connectAccountsService;
            var _wrapper = wrapper;
            _stripeSecretAPIKey = _wrapper.Value;
        }

        public bool CreateStripeCustomer(string userId, string userEmail)
        {
            StripeConfiguration.ApiKey = _stripeSecretAPIKey;
            var connectAccountId = _connectAccountsService.GetConnectedAccountId();
            var customerService = new CustomerService();
            var customerOptions = new CustomerCreateOptions
            {
                Email = userEmail,
                Description = "Customer for " + userEmail,
                Metadata = new Dictionary<string, string>
                {
                    { "UserId", userId },
                }
            };

            var requestOptions = new RequestOptions();
            requestOptions.StripeAccount = connectAccountId;

            try
            {
                var stripeCustomer = customerService.Create(customerOptions, requestOptions);

                var hasAssigned = _usersSrv.AssignStripeCustomerId(userId, stripeCustomer.Id);
                if (!hasAssigned)
                {
                    _logger.LogError($"Error creating Stripe customer for user: {userId}. A stripe customer with an ID of: {stripeCustomer.Id} has been created, but it's Id has not been assigned to SQL column");
                    return false;
                }
            }
            catch (StripeException ex)
            {
                _logger.LogError($"Error creating Stripe customer for user: {userId}. Error: {ex.Message}");
                return false;
            }
            return true;
        }
    }
}
