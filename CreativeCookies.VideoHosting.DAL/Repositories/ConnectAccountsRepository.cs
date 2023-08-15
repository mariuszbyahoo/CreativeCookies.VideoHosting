using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.DAL.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace CreativeCookies.VideoHosting.DAL.Repositories
{
    public class ConnectAccountsRepository : IConnectAccountsRepository
    {
        private readonly AppDbContext _ctx;
        private readonly IConfiguration _configuration;
        private readonly string _stripeSecretAPIKey;
        private readonly ILogger<ConnectAccountsRepository> _logger;
        public ConnectAccountsRepository(AppDbContext ctx, IConfiguration configuration, ILogger<ConnectAccountsRepository> logger)
        {
            _ctx = ctx;
            _configuration = configuration;
            _logger = logger;
            _stripeSecretAPIKey = _configuration.GetValue<string>("StripeSecretAPIKey");
        }
        public async Task<string> GetConnectedAccountId()
        {
            var record = await _ctx.StripeAccountRecords.FirstOrDefaultAsync();
            if (record == null) return string.Empty;
            return record.StripeConnectedAccountId;
        }

        public async Task EnsureSaved(string accountId)
        {
            var lookup = _ctx.StripeAccountRecords.FirstOrDefault(a => a.Id.Equals(accountId));
            if (lookup == null)
            {
                await DeleteStoredAccounts(string.Empty);
            }
            else await DeleteStoredAccounts(lookup.StripeConnectedAccountId);

            await SaveConnectedAccount(accountId);
        }

        private async Task SaveConnectedAccount(string accountId)
        {
            var newAccountRecord = new DAL.DAOs.StripeAccountRecord() { Id = Guid.NewGuid(), StripeConnectedAccountId = accountId, DateCreated = DateTime.UtcNow };
            _ctx.StripeAccountRecords.Add(newAccountRecord);
            await _ctx.SaveChangesAsync();
        }

        private async Task DeleteStoredAccounts(string accountToPersist)
        {
            try
            {
                var list = await _ctx.StripeAccountRecords.ToListAsync();
                for (int i = 0; i < list.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(accountToPersist) && !list[i].Id.Equals(accountToPersist))
                    {
                        _ctx.Remove(list[i]);
                    }
                    else
                    {
                        _ctx.Remove(list[i]);
                    }
                }
                await _ctx.SaveChangesAsync();
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "And error occured while deleting the account Ids from the database - not all entities has been removed");
            }
        }

        public async Task<bool> CanBeQueriedOnStripe(string accountId)
        {
            var lookup = await _ctx.StripeAccountRecords.Where(a => a.StripeConnectedAccountId.Equals(accountId)).FirstOrDefaultAsync();
            if (lookup == null) return true;
            else return DateTime.UtcNow - lookup.DateCreated > TimeSpan.FromMinutes(1);
        }
    }
}
