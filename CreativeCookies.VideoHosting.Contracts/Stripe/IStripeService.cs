using CreativeCookies.VideoHosting.Contracts.Enums;
using CreativeCookies.VideoHosting.Contracts.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Stripe
{
    public interface IStripeService
    {
        public IStripeResult<StripeConnectAccountStatus> GetAccountStatus(string idStoredInDatabase);
        public IStripeResult<string> GetConnectAccountLink();
    }
}
