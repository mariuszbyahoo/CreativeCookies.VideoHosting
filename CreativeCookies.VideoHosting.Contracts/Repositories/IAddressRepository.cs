using CreativeCookies.VideoHosting.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Repositories
{
    public interface IAddressRepository
    {
        Task<AddressDto?> GetAddress(string userId);
        Task<AddressDto?> GetAddress(Guid addressId);

        /// <summary>
        /// Saves new Address to the database
        /// </summary>
        /// <param name="address">Address to save</param>
        /// <returns>Amount of entities created</returns>
        Task<int> AddAddress(AddressDto address);

        /// <summary>
        /// Updates existing address
        /// </summary>
        /// <param name="newAddress">Address referencing existing entity with Id property</param>
        /// <returns>If all went good - amount of entities updated, if no address to update found throws KeyNotFoundException</returns>
        Task<int> UpdateAddress(AddressDto newAddress);
    }
}
