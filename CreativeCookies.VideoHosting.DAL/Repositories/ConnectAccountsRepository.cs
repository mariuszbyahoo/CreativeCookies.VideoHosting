﻿using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.DAL.DAOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace CreativeCookies.VideoHosting.DAL.Repositories
{
    public class ConnectAccountsRepository : IConnectAccountsRepository
    {
        private readonly AppDbContext _ctx;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConnectAccountsRepository> _logger;
        public ConnectAccountsRepository(AppDbContext ctx, IConfiguration configuration, ILogger<ConnectAccountsRepository> logger)
        {
            _ctx = ctx;
            _configuration = configuration;
            _logger = logger;
        }
        public string GetConnectedAccountId()
        {
            _logger.LogInformation("Collecting StripeConnectAccountId");
            var record = _ctx.StripeConfig.FirstOrDefault();
            _logger.LogInformation($"Collected StripeConnectAccountId: {record?.StripeConnectedAccountId}");
            if (record == null) return string.Empty;
            return record.StripeConnectedAccountId;
        }

        public async Task EnsureSaved(string accountId)
        {
            var lookup = _ctx.StripeConfig.FirstOrDefault(a => a.Id.Equals(accountId));
            if (lookup == null)
            {
                await DeleteStoredAccounts(string.Empty);
            }
            else await DeleteStoredAccounts(lookup.StripeConnectedAccountId);

            await SaveConnectedAccount(accountId);
        }

        private async Task SaveConnectedAccount(string accountId)
        {
            var newAccountRecord = new DAL.DAOs.StripeConfig() { Id = Guid.NewGuid(), StripeConnectedAccountId = accountId, DateCreated = DateTime.UtcNow };
            _ctx.StripeConfig.Add(newAccountRecord);
            _ctx.SaveChanges();
        }

        private async Task DeleteStoredAccounts(string accountToPersist)
        {
            try
            {
                var list = await _ctx.StripeConfig.ToListAsync();
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
                _ctx.SaveChanges();
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "And error occured while deleting the account Ids from the database - not all entities has been removed");
            }
        }

        public async Task<bool> CanBeQueriedOnStripe(string accountId)
        {
            var lookup = await _ctx.StripeConfig.Where(a => a.StripeConnectedAccountId.Equals(accountId)).FirstOrDefaultAsync();
            if (lookup == null) return true;
            else return DateTime.UtcNow - lookup.DateCreated > TimeSpan.FromMinutes(1);
        }
    }
}
