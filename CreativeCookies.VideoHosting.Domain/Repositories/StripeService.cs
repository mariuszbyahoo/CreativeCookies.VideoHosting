using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.DAL.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace CreativeCookies.VideoHosting.Domain.Repositories
{
    public class StripeService : IStripeService
    {
        private readonly AppDbContext _ctx;
        private readonly IConfiguration _configuration;
        private readonly string _stripeSecretAPIKey;
        public StripeService(AppDbContext ctx, IConfiguration configuration)
        {
            _ctx = ctx;
            _configuration = configuration;
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
    }
}
