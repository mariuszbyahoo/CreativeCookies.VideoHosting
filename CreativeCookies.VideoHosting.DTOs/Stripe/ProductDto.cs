using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DTOs.Stripe
{
    public class ProductDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IList<PriceDto> Prices { get; set; } = new List<PriceDto>();

        public ProductDto(string id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }
    }

}
