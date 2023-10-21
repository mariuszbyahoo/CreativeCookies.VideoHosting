using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.DTOs.About;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DAL.Repositories
{
    public class AboutPageRepository : IAboutPageRepository
    {
        private readonly AppDbContext _ctx;

        public AboutPageRepository(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<AboutPageDTO> GetAboutPageInnerHTML()
        {
            AboutPageDTO result = new AboutPageDTO();

            var record = await _ctx.AboutPageContent.FirstOrDefaultAsync();
            if (record != null) result.InnerHTML = record.InnerHtml;

            return result;
        }

        public async Task<bool> UpsertAboutPageInnerHTML(string newInnerHtml)
        {
            var record = await _ctx.AboutPageContent.FirstOrDefaultAsync();
            if (record == null)
            {
                record = new DAOs.AboutPageContent();
                record.InnerHtml = newInnerHtml;
                await _ctx.AboutPageContent.AddAsync(record);
                var res = await _ctx.SaveChangesAsync();
                return res > 0;
            }
            else
            {
                record.InnerHtml = newInnerHtml;
                var res = await _ctx.SaveChangesAsync();
                return res > 0;
            }
        }
    }
}
