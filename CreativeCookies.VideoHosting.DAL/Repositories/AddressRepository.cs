using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.DAL.DAOs;
using CreativeCookies.VideoHosting.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DAL.Repositories
{
    public class AddressRepository : IAddressRepository
    {
        private readonly AppDbContext _ctx;

        public AddressRepository(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<int> AddAddress(AddressDto address)
        {
            var dao = new Address(address.FirstName, address.LastName, 
                address.Street, address.HouseNo, address.PostCode, 
                address.City, address.Country, address.UserId);
            
            if (address.AppartmentNo != null) dao.AppartmentNo = address.AppartmentNo;

            _ctx.Addresses.Add(dao);
            return await _ctx.SaveChangesAsync();
        }

        public async Task<AddressDto?> GetAddress(string userId)
        {
            var lowerCaseUserId = userId.ToLowerInvariant();
            var dao = await _ctx.Addresses.Where(a => a.UserId == lowerCaseUserId).FirstOrDefaultAsync();
            if (dao == null) return null;
            var dto = new AddressDto(dao.Id, dao.FirstName, dao.LastName, 
                dao.Street, dao.HouseNo, dao.AppartmentNo, dao.PostCode, 
                dao.City, dao.Country, dao.UserId);
            return dto;
        }

        public async Task<AddressDto?> GetAddress(Guid addressId)
        {
            var dao = await _ctx.Addresses.FindAsync(addressId);
            if (dao == null) return null;
            var dto = new AddressDto(dao.Id, dao.FirstName, dao.LastName, 
                dao.Street, dao.HouseNo, dao.AppartmentNo, dao.PostCode, 
                dao.City, dao.Country, dao.UserId);
            return dto;
        }

        public async Task<int> UpdateAddress(AddressDto updatedAddress)
        {
            var address = await _ctx.Addresses.FindAsync(updatedAddress.Id);
            if (address == null)
            {
                throw new KeyNotFoundException("Address not found.");
            }

            address.FirstName = updatedAddress.FirstName;
            address.LastName = updatedAddress.LastName;
            address.Street = updatedAddress.Street;
            address.HouseNo = updatedAddress.HouseNo;
            address.AppartmentNo = updatedAddress.AppartmentNo;
            address.PostCode = updatedAddress.PostCode;
            address.City = updatedAddress.City;
            address.Country = updatedAddress.Country;
            address.UserId = updatedAddress.UserId;

            return await _ctx.SaveChangesAsync();
        }
    }
}
