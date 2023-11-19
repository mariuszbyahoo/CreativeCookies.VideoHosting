using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.DAL.DAOs;
using CreativeCookies.VideoHosting.DTOs.Regulations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DAL.Repositories
{
    public class RegulationsRepository : IRegulationsRepository
    {
        private readonly AppDbContext _ctx;

        public RegulationsRepository(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<WebsitePrivacyPolicyDTO> GetPrivacyPolicy()
        {
            var result = new WebsitePrivacyPolicyDTO();
            var dao =  await _ctx.PrivacyPolicies.FirstOrDefaultAsync();
            if (dao == null)
            {
                result.HtmlContent = string.Empty;
            }
            else
            {
                result.HtmlContent = dao.HtmlContent;
            }

            return result;
        }

        public async Task<WebsiteRegulationsDTO> GetRegulations()
        {
            var result = new WebsiteRegulationsDTO();
            var dao = await _ctx.Regulations.FirstOrDefaultAsync();
            if (dao == null)
            {
                result.HtmlContent = string.Empty;
            }
            else
            {
                result.HtmlContent = dao.HtmlContent;
            }

            return result;
        }

        public async Task<WebsitePrivacyPolicyDTO> UpdatePrivacyPolicy(WebsitePrivacyPolicyDTO dto)
        {
            var dao = await _ctx.PrivacyPolicies.FirstOrDefaultAsync();
            if(dao == null)
            {
                dao = new WebsitePrivacyPolicy();
            }
            dao.HtmlContent = dto.HtmlContent;
            var res = _ctx.SaveChanges();
            if(res != 0)
            {
                return new WebsitePrivacyPolicyDTO() { HtmlContent = dao.HtmlContent };
            }
            return new WebsitePrivacyPolicyDTO();
        }

        public async Task<WebsiteRegulationsDTO> UpdateRegulaions(WebsiteRegulationsDTO dto)
        {
            var dao = await _ctx.Regulations.FirstOrDefaultAsync();
            if (dao == null)
            {
                dao = new WebsiteRegulations();
            }
            dao.HtmlContent = dto.HtmlContent;
            var res = _ctx.SaveChanges();
            if (res != 0)
            {
                return new WebsiteRegulationsDTO() { HtmlContent = dao.HtmlContent };
            }
            return new WebsiteRegulationsDTO();
        }
    }
}
