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
    public class MerchantService : IMerchantService
    {
        private readonly IMerchantRepository _repo;

        public MerchantService(IMerchantRepository repo)
        {
            _repo = repo;
        }

        public async Task<MerchantDto?> GetMerchant()
        {
            return await _repo.GetMerchant();
        }

        public async Task<int> UpsertMerchant(MerchantDto newMerchant)
        {
            var lookup = await _repo.GetMerchant();

            if (lookup == null) return await _repo.AddMerchant(newMerchant);
            else return await _repo.UpdateMerchant(newMerchant);
        }
    }
}
