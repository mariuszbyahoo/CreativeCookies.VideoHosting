using CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe;
using CreativeCookies.VideoHosting.Contracts.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;

namespace CreativeCookies.VideoHosting.Infrastructure.Stripe
{
    public class StripeCustomerService : IStripeCustomerService
    {
        private readonly IUsersService _usersSrv;
        private readonly IConnectAccountsService _connectAccountsService;
        private readonly ILogger<StripeCustomerService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _stripeSecretAPIKey;

        public StripeCustomerService(IUsersService usersSrv, ILogger<StripeCustomerService> logger, IConnectAccountsService connectAccountsService,
            IConfiguration configuration)
        {
            _usersSrv = usersSrv;
            _logger = logger;
            _configuration = configuration;
            _connectAccountsService = connectAccountsService;
            _stripeSecretAPIKey = _configuration.GetValue<string>("StripeSecretAPIKey");
        }

        public async Task<bool> CreateStripeCustomer(string userId, string userEmail)
        {
            StripeConfiguration.ApiKey = _stripeSecretAPIKey;
            var customerService = new CustomerService();
            var customerOptions = new CustomerCreateOptions
            {
                Email = userEmail,
                Description = "Customer for " + userEmail,
                Metadata = new Dictionary<string, string>
                {
                    { "UserId", userId },
                    { "StripeConnectedAccountId", await _connectAccountsService.GetConnectedAccountId() }
                }
            };

            try
            {
                var stripeCustomer = customerService.Create(customerOptions);

                var hasAssigned = await _usersSrv.AssignStripeCustomerId(userId, stripeCustomer.Id);
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
