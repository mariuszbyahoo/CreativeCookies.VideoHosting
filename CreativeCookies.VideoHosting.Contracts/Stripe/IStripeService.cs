using CreativeCookies.VideoHosting.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Stripe
{
    public interface IStripeService
    {
        public StripeConnectAccountStatus GetAccountStatus(string idStoredInDatabase);
        public Task<string> GetConnectAccountLink();
    }
}
