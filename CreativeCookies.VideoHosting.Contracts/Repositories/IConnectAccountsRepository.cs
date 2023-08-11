using CreativeCookies.VideoHosting.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Repositories
{
    public interface IConnectAccountsRepository
    {
        /// <summary>
        /// Checks is there any Stripe Connected account added to the database
        /// </summary>
        /// <returns>Account's Stripe Id, or empty string if found none</returns>
        public Task<string> GetConnectedAccountId();

        /// <summary>
        /// Checks is Id stored in the Database also present in Stripe's connected accounts set.
        /// </summary>
        /// <returns>true - DB contains valid Stripe Connect Id (at least one), false - DB holds no records</returns>
        public StripeConnectAccountStatus ReturnAccountStatus(string IdStoredInDatabase);

        public Task<string> ReturnConnectAccountLink();

        public Task<bool> SaveAccountId(string accountId);

        /// <summary>
        /// Removes all stored Stripe Account Ids from the database
        /// </summary>
        /// <returns></returns>
        public Task DeleteConnectAccounts();

        /// <summary>
        /// Checks, is there anything in the database stored already.
        /// </summary>
        /// <returns></returns>
        public bool HasAnyEntity();
    }
}
