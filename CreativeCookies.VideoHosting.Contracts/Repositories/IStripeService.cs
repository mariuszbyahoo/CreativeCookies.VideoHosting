using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Repositories
{
    public interface IStripeService
    {
        /// <summary>
        /// Checks is there any Stripe Connected account added to the database
        /// </summary>
        /// <returns>Account's Stripe Id, or empty string if found none</returns>
        public Task<string> GetConnectedAccountsId();

        /// <summary>
        /// Checks is Id stored in the Database also present in Stripe's connected accounts set.
        /// </summary>
        /// <returns>true - DB contains valid Stripe Connect Id value, false - Stripe Connect ID account has not been found in Stripe API</returns>
        public bool IsDbRecordValid(string IdStoredInDatabase);

        public string ReturnConnectAccountLink();

        public Task<bool> SaveAccountId(string accountId);
    }
}
