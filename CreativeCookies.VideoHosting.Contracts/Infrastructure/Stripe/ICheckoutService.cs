using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe
{
    public interface ICheckoutService
    {
        public Task<string> CreateNewSession(string priceId, string stripeCustomerId, bool HasDeclinedCoolingOffPeriod = false);

        public Task<bool> IsSessionPaymentPaid(string sessionId);

        Task<string> CreateDeferredSubscription(string customerId, string priceId);
    }
}
