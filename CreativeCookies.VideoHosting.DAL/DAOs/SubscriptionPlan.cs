using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DAL.DAOs
{
    [PrimaryKey(nameof(StripeProductId))]
    public class SubscriptionPlan
    {
        public string StripeProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IList<Price> StripePrices { get; set; }
    }
}
