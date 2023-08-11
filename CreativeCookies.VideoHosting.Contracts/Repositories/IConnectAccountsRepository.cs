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
