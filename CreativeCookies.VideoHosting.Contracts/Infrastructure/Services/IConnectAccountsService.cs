using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Infrastructure.Services
{
    public interface IConnectAccountsService
    {
        /// <summary>
        /// Checks is there any Stripe Connected account added to the database
        /// </summary>
        /// <returns>Account's Stripe Id, or empty string if found none</returns>
        public Task<string> GetConnectedAccountId();

        /// <summary>
        /// Checks, is an account eligible to be checked on the Stripe's API side. 
        /// </summary>
        /// <returns>bool indicating, is an account older than one minute (UTC time) - if not, false.</returns>
        public Task<bool> CanBeQueriedOnStripe(string accountId);

        /// <summary>
        /// Looks up the database is there any account with specified Id, if not, saves it to db.
        /// if there is an existing record with such an accountId like in param, then ignores
        /// if there is an existing record with anouter ID than this like in params, then cleans the DB table and saves a new one.
        /// </summary>
        /// <param name="accountId"></param>
        public Task EnsureSaved(string accountId);
    }
}
