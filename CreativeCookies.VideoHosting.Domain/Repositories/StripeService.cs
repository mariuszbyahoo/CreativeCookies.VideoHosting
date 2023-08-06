using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.DAL.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Domain.Repositories
{
    public class StripeService : IStripeService
    {
        private readonly AppDbContext _ctx;
        public StripeService(AppDbContext ctx)
        {
            _ctx = ctx;
        }
        public async Task<string> GetConnectedAccountsId()
        {
            var record = await _ctx.StripeAccountRecords.FirstOrDefaultAsync();
            if(record == null) return string.Empty;
            return record.StripeConnectedAccountId;
        }
    }
}
