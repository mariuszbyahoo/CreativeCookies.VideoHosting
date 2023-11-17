using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DTOs
{
    public class AddressDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Street { get; set; }
        public string HouseNo { get; set; }
        public int? AppartmentNo { get; set; }
        public string PostCode { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }
}
