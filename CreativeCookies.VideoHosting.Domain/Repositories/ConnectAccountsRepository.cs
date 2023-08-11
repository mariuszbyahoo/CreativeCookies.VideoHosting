using CreativeCookies.VideoHosting.Contracts.Enums;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.DAL.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CreativeCookies.VideoHosting.Domain.Repositories
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
            if(record == null) return string.Empty;
            return record.StripeConnectedAccountId;
        }

        public async Task<bool> SaveAccountId(string accountId)
        {
            try
            {
                await SaveConnectedAccount(accountId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "And error occured while saving account Id to the database");
                return false;
            }
        }

        private async Task SaveConnectedAccount(string accountId)
        {
            var newAccountRecord = new DAL.DAOs.StripeAccountRecord() { Id = Guid.NewGuid(), StripeConnectedAccountId = accountId };
            _ctx.StripeAccountRecords.Add(newAccountRecord);
            await _ctx.SaveChangesAsync();
        }

        public async Task DeleteConnectAccounts()
        {
            try
            {
                StripeConfiguration.ApiKey = _stripeSecretAPIKey;


                var list = await _ctx.StripeAccountRecords.ToListAsync();
                for(int i = 0; i < list.Count; i++)
                {
                    _ctx.Remove(list[i]);
                }
                await _ctx.SaveChangesAsync();
            }

            catch(Exception ex)
            {
                _logger.LogError(ex, "And error occured while deleting the account Ids from the database - entities has not been removed");
            }
        }

        public bool HasAnyEntity()
        {
            return _ctx.StripeAccountRecords.ToList().Any();
        }
    }
}
