using CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Infrastructure.Stripe
{
    public class ApplicationFeeWrapper 
    {
        public decimal NettApplicationFee { get; set; }

        public ApplicationFeeWrapper(decimal nettApplicationFee)
        {
            NettApplicationFee = nettApplicationFee;
        }
    }
}
