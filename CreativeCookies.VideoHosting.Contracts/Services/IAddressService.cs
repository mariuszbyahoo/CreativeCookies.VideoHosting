using CreativeCookies.VideoHosting.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Services
{
    public interface IAddressService
    {
        Task<InvoiceAddressDto?> GetAddress(string userId);

        /// <summary>
        /// Checks, is there an entity with address.Id present in the database, if so - updates it
        /// If not - adds new address
        /// </summary>
        /// <param name="address">Address to upsert</param>
        /// <returns>Amount of entities updated</returns>
        Task<int> UpsertAddress(InvoiceAddressDto address);
    }
}
