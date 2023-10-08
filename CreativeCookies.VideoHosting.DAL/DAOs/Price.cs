using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DAL.DAOs
{
    [PrimaryKey(nameof(StripePriceId))]
    public class Price
    {
        public string StripePriceId { get; set; }
        public string CurrencyCode { get; set; }
        public int Amount { get; set; }
        public bool IsActive { get; set; }
        [ForeignKey("SubscriptionPlan")]
        public string StripeProductId { get; set; }
        [Required]
        public SubscriptionPlan SubscriptionPlan { get; set; }
    }
}
