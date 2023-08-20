using CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.Services.IdP;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Infrastructure.Stripe
{
    public class StripeCustomerService : IStripeCustomerService
    {
        private readonly IUsersRepository _usersRepo;
        private readonly ILogger<StripeCustomerService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _stripeSecretAPIKey;

        public StripeCustomerService(IUsersRepository usersRepo, ILogger<StripeCustomerService> logger, IConfiguration configuration)
        {
            _usersRepo = usersRepo;
            _logger = logger;
            _configuration = configuration;
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
                    { "UserId", userId }
                }
            };

            try
            {
                var stripeCustomer = customerService.Create(customerOptions);

                var hasAssigned = await _usersRepo.AssignStripeCustomerId(userId, stripeCustomer.Id);
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
