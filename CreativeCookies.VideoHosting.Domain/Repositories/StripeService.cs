using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.DAL.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;

namespace CreativeCookies.VideoHosting.Domain.Repositories
{
    public class StripeService : IStripeService
    {
        private readonly AppDbContext _ctx;
        private readonly IConfiguration _configuration;
        private readonly string _stripeSecretAPIKey;
        private readonly ILogger<StripeService> _logger;
        public StripeService(AppDbContext ctx, IConfiguration configuration, ILogger<StripeService> logger)
        {
            _ctx = ctx;
            _configuration = configuration;
            _logger = logger;
            _stripeSecretAPIKey = _configuration.GetValue<string>("StripeSecretAPIKey");
        }
        public async Task<string> GetConnectedAccountsId()
        {
            var record = await _ctx.StripeAccountRecords.FirstOrDefaultAsync();
            if(record == null) return string.Empty;
            return record.StripeConnectedAccountId;
        }

        public bool IsDbRecordValid(string idStoredInDatabase)
        {
            StripeConfiguration.ApiKey = _stripeSecretAPIKey;
           
            var service = new AccountService();
            var list = service.List();
            if(list != null)
            {
                return list.FirstOrDefault(a => a.Id.Equals(idStoredInDatabase)) != null;
            }
            return false;
        }

        public string ReturnConnectAccountLink()
        {
            try
            {
                StripeConfiguration.ApiKey = _stripeSecretAPIKey;
                var apiUrl = _configuration.GetValue<string>("ApiUrl");
                var accountOptions = new AccountCreateOptions { Type = "standard" };
                var accountSrv = new AccountService();
                var account = accountSrv.Create(accountOptions);

                var linkOptions = new AccountLinkCreateOptions
                {
                    Account = account.Id,
                    RefreshUrl = $"{apiUrl}/api/Stripe/OnboardingRefresh",
                    ReturnUrl = $"{apiUrl}/api/Stripe/OnboardingReturn",
                    Type = "account_onboarding",
                };
                var accountLinkSrv = new AccountLinkService();
                var link = accountLinkSrv.Create(linkOptions);
                return link.Url;
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Error occured in ReturnConnectAccountLink()");
                return string.Empty;
            }
        }

        public async Task<bool> SaveAccountId(string accountId)
        {
            try
            {
                var newAccountRecord = new DAL.DAOs.StripeAccountRecord() { Id = Guid.NewGuid(), StripeConnectedAccountId = accountId };
                _ctx.StripeAccountRecords.Add(newAccountRecord);
                _ctx.SaveChangesAsync();
                return true;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "And error occured while saving account Id to the database");
                return false;
            }
        }
    }
}
