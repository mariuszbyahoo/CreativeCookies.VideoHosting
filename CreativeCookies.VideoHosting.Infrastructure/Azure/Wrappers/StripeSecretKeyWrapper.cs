using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Infrastructure.Azure.Wrappers
{
    public class StripeSecretKeyWrapper
    {
        public string Value { get; }
        public StripeSecretKeyWrapper(string value)
        {
            Value = value;
        }
    }
}
