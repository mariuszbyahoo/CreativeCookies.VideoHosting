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
    }
}
