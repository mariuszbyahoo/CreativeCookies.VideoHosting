using CreativeCookies.VideoHosting.DAL.DAOs.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DAL.DAOs
{
    public class Address
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Street { get; set; }
        public string HouseNo { get; set; }
        public int? AppartmentNo { get; set; }
        public string PostCode { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string UserId { get; set; }
        public MyHubUser User { get; set; }

        public Address()
        {

        }

        public Address(string firstName, string lastName, string street, string houseNo, string postCode, string city, string country, string userId)
        {
            FirstName = firstName;
            LastName = lastName;
            Street = street;
            HouseNo = houseNo;
            PostCode = postCode;
            City = city;
            Country = country;
            UserId = userId;
        }
        public Address(string firstName, string lastName, string street, string houseNo, int appartmentNo, string postCode, string city, string country, string userId)
        {
            FirstName = firstName;
            LastName = lastName;
            Street = street;
            HouseNo = houseNo;
            PostCode = postCode;
            City = city;
            Country = country;
            AppartmentNo = appartmentNo;
            UserId = userId;
        }
    }
}
