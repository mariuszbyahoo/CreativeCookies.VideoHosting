using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DTOs.Films
{
    public class SubscriptionDateRange
    {
        public string DateRanges { get; private set; }
        public DateTime StartDateUTC { get; set; }
        public DateTime EndDateUTC { get; set; }

        public SubscriptionDateRange(DateTime startDateUTC, DateTime endDateUTC)
        {
            StartDateUTC = startDateUTC;
            EndDateUTC = endDateUTC;
            DateRanges = $"{StartDateUTC} - {EndDateUTC}";
        }
    }
}
