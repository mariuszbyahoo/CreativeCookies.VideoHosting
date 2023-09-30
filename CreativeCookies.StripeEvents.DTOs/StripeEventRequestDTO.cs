using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.StripeEvents.DTOs
{
    public class StripeEventRequestDTO
    {
        public string StripeSignature { get; set; }
        public string JsonRequestBody { get; set; }
    }
}
