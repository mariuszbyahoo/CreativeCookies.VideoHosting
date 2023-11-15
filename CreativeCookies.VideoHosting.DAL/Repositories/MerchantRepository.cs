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
    public class MerchantRepository : IMerchantRepository
    {
        private readonly AppDbContext _ctx;
        public MerchantRepository(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<int> AddMerchant(MerchantDto newMerchant)
        {
            var dao = new Merchant(newMerchant.CompanyName, 
                newMerchant.CompanyTaxId, newMerchant.Street, newMerchant.HouseNo, 
                newMerchant.AppartmentNo, newMerchant.PostCode, newMerchant.City, 
                newMerchant.Country, newMerchant.IsVATExempt);
            _ctx.Merchant.Add(dao);
            return await _ctx.SaveChangesAsync();
        }

        public async Task<MerchantDto?> GetMerchant()
        {
            var dao = await _ctx.Merchant.FirstOrDefaultAsync();
            if (dao == null) return null;
            var res = new MerchantDto(dao.Id, dao.CompanyName, 
                dao.CompanyTaxId, dao.Street, dao.HouseNo, dao.AppartmentNo, 
                dao.PostCode, dao.City, dao.Country, dao.IsVATExempt);
            return res;
        }

        public async Task<int> UpdateMerchant(MerchantDto newMerchant)
        {
            var dao = await _ctx.Merchant.FirstOrDefaultAsync();
            dao.CompanyName = newMerchant.CompanyName;
            dao.CompanyTaxId = newMerchant.CompanyTaxId;
            dao.Street = newMerchant.Street;
            dao.HouseNo = newMerchant.HouseNo;
            dao.AppartmentNo = newMerchant.AppartmentNo;
            dao.PostCode = newMerchant.PostCode;
            dao.City = newMerchant.City;
            dao.Country = newMerchant.Country;
            dao.IsVATExempt = newMerchant.IsVATExempt;

            return await _ctx.SaveChangesAsync();
        }
    }
}
