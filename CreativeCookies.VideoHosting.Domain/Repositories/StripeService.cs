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

        public StripeConnectAccountStatus ReturnAccountStatus(string idStoredInDatabase)
        {
            StripeConfiguration.ApiKey = _stripeSecretAPIKey;
            var result = StripeConnectAccountStatus.Disconnected;

            var service = new AccountService();
            Account stripeAccount = service.Get(idStoredInDatabase); // get the specific account by ID

            if (stripeAccount != null)
            {
                if (stripeAccount.Capabilities != null &&
                   stripeAccount.Capabilities.CardPayments != null && stripeAccount.Capabilities.CardPayments.Equals("active") &&
                   stripeAccount.Capabilities.Transfers != null && stripeAccount.Capabilities.Transfers.Equals("active"))
                {
                    if (stripeAccount.Requirements != null &&
                       (stripeAccount.Requirements.PastDue == null || !stripeAccount.Requirements.PastDue.Any()))
                    {
                        result = StripeConnectAccountStatus.Connected;
                    }
                    else
                    {
                        result = StripeConnectAccountStatus.Restricted;
                    }
                }
                else if(stripeAccount.Capabilities != null && !stripeAccount.DetailsSubmitted)
                {
                    result = StripeConnectAccountStatus.Restricted;
                }
            }
            return result;
        }


        public async Task<string> ReturnConnectAccountLink()
        {
            try
            {
                StripeConfiguration.ApiKey = _stripeSecretAPIKey;
                var apiUrl = _configuration.GetValue<string>("ApiUrl");
                var clientUrl = _configuration.GetValue<string>("ClientUrl");

                // Check if an account already exists
                var existingAccount = _ctx.StripeAccountRecords.FirstOrDefault();
                string accountId = existingAccount?.StripeConnectedAccountId;

                if (accountId == null)
                {
                    var accountOptions = new AccountCreateOptions { Type = "standard" };
                    var accountSrv = new AccountService();
                    var account = accountSrv.Create(accountOptions);
                    await SaveConnectedAccount(account.Id);
                }

                var linkOptions = new AccountLinkCreateOptions
                {
                    Account = accountId,
                    RefreshUrl = $"{apiUrl}/api/Stripe/OnboardingRefresh",
                    ReturnUrl = $"{clientUrl}/stripeOnboardingReturn",
                    Type = "account_onboarding",
                };
                var accountLinkSrv = new AccountLinkService();
                var link = accountLinkSrv.Create(linkOptions);
                return link.Url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in ReturnConnectAccountLink()");
                return string.Empty;
            }
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
