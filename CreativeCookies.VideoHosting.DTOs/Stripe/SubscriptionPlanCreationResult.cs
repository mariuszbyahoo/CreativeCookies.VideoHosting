using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DTOs.Stripe
{
    public class SubscriptionPlanCreationResult
    {
        public string StripeProductId { get; set; }

        public SubscriptionPlanCreationResult(string stripeProductId)
        {
            StripeProductId = stripeProductId;
        }
    }
}
