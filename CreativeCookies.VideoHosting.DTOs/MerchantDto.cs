using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DTOs
{
    public class MerchantDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string CompanyTaxId { get; set; }
        public string Street { get; set; }
        public string HouseNo { get; set; }
        public int? AppartmentNo { get; set; }
        public string PostCode { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        public bool IsVATExempt { get; set; }

        public MerchantDto(int id, string companyName, string companyTaxId, string street, string houseNo, int? appartmentNo, string postCode, string city, string country, bool isVatExempt)
        {
            Id = id;
            CompanyName = companyName;
            CompanyTaxId = companyTaxId;
            Street = street;
            HouseNo = houseNo;
            AppartmentNo = appartmentNo;
            PostCode = postCode;
            City = city;
            Country = country;
            IsVATExempt = isVatExempt;
        }
    }
}
