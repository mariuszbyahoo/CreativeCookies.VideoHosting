using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DTOs.Stripe
{
    public class SubscriptionPlanDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsLinked { get; set; }
        public IList<PriceDto> Prices { get; set; } = new List<PriceDto>();

        public SubscriptionPlanDto(string id, string name, string description, bool isLinked = false)
        {
            Id = id;
            Name = name;
            Description = description;
            IsLinked = isLinked;
        }
    }

}
