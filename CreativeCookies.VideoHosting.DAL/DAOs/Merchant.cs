using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DAL.DAOs
{
    public class Merchant
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

        /// <summary>
        /// If true - VAT amount on invoices should be zw. (0%), else - standard VAT 23%
        /// </summary>
        public bool IsVATExempt { get; set; }

        public Merchant(string companyName, string companyTaxId, string street, string houseNo, int? appartmentNo, string postCode, string city, string country, bool isVATExempt)
        {
            CompanyName = companyName;
            CompanyTaxId = companyTaxId;
            Street = street;
            HouseNo = houseNo;
            AppartmentNo = appartmentNo;
            PostCode = postCode;
            City = city;
            Country = country;
            IsVATExempt = isVATExempt;
        }
    }
}
