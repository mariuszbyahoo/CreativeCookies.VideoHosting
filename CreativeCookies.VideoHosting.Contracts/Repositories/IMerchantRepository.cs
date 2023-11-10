using CreativeCookies.VideoHosting.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Repositories
{
    public interface IMerchantRepository
    {
        public Task<MerchantDto?> GetMerchant();

        public Task<int> AddMerchant(MerchantDto newMerchant);

        public Task<int> UpdateMerchant(MerchantDto newMerchant);
    }
}
