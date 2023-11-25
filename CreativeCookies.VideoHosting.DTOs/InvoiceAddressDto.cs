using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DTOs
{
    public class InvoiceAddressDto : AddressDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }

        public InvoiceAddressDto(Guid id, string firstName, 
            string lastName, string street, string houseNo, 
            int? appartmentNo, string postCode, 
            string city, string country, string userId)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Street = street;
            HouseNo = houseNo;
            AppartmentNo = appartmentNo;
            PostCode = postCode;
            City = city;
            Country = country;
            UserId = userId;
        }
        public InvoiceAddressDto()
        {

        }

        public override bool Equals(object obj)
        {
            return obj is InvoiceAddressDto dto &&
                   Id == dto.Id &&
                   UserId == dto.UserId &&
                   HouseNo == dto.HouseNo &&
                   AppartmentNo == dto.AppartmentNo &&
                   City == dto.City &&
                   Country == dto.Country &&
                   FirstName == dto.FirstName &&
                   LastName == dto.LastName &&
                   PostCode == dto.PostCode &&
                   Street == dto.Street;
        }

        public override int GetHashCode()
        {
            // Hence HashCode.Combine takes only max 8 props, then use nested hashing
            int hash1 = HashCode.Combine(Id, UserId, HouseNo, AppartmentNo, City, Country);
            int hash2 = HashCode.Combine(FirstName, LastName, PostCode, Street, hash1);

            return hash2;
        }
    }
}
