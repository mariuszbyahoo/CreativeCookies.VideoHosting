using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Infrastructure.Azure.Wrappers
{
    public class StripeWebhookSigningKeyWrapper
    {
        public string Value { get; set; }

        public StripeWebhookSigningKeyWrapper(string value)
        {
            Value = value;
        }
    }
}
