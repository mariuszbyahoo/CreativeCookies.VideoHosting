using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe
{
    public interface ICheckoutService
    {
        Task<bool> HasUserActiveSubscription(string customerId);
        Task<string> CreateNewSession(string priceId, string stripeCustomerId, bool HasDeclinedCoolingOffPeriod = false);

        Task<bool> IsSessionPaymentPaid(string sessionId);

        /// <summary>
        /// Creates subscription with specified TrialPeriod
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="priceId"></param>
        /// <param name="TrialPeriod">Trial period after which user will be charged normally</param>
        /// <returns></returns>
        string CreateDeferredSubscription(string customerId, string priceId, TimeSpan TrialPeriod);
        /// <summary>
        /// Initiates full refund for customer - use in case of cancellation within 14 days cooling off period
        /// </summary>
        /// <param name="userId">UserId of a customer to cancell the order for</param>
        /// <returns>true - operation succeed, otherwise false</returns>
        Task<bool> RefundCanceledOrder(string userId);

        Task<bool> CancelSubscription(string userId);
    }
}
