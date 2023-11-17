using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.Services;
using CreativeCookies.VideoHosting.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Services
{
    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _repo;

        public AddressService(IAddressRepository repo)
        {
            _repo = repo;
        }

        public async Task<InvoiceAddressDto?> GetAddress(string userId)
        {
            return await _repo.GetAddress(userId);
        }

        public async Task<int> UpsertAddress(InvoiceAddressDto address)
        {
            var lookup = await _repo.GetAddress(address.Id);
            if(lookup == null)
            {
                return await _repo.AddAddress(address);
            }
            else
            {
                return await _repo.UpdateAddress(address);
            }
        }
    }
}
