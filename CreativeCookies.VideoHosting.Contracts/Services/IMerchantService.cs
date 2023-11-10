using CreativeCookies.VideoHosting.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Services
{
    public interface IMerchantService
    {
        public Task<MerchantDto?> GetMerchant();

        public Task<int> UpsertMerchant(MerchantDto newMerchant);
    }
}
