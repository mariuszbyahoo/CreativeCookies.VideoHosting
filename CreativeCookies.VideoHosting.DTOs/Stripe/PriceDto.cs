using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DTOs.Stripe
{
    public class PriceDto
    {
        public string Id { get; set; }
        public bool IsActive { get; set; }
        public string ProductId { get; set; }
        public string Currency { get; set; }
        public long? UnitAmount { get; set; }
        public string RecurringInterval { get; set; }

        public PriceDto(string id, bool isActive, string productId, string currency, long? unitAmount, string recurringInterval)
        {
            Id = id;
            IsActive = isActive;
            ProductId = productId;
            Currency = currency;
            UnitAmount = unitAmount;
            RecurringInterval = recurringInterval;
        }
    }
}
