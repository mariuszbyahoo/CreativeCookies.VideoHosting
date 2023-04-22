using CreativeCookies.VideoHosting.Contracts.DTOs.OAuth;
using CreativeCookies.VideoHosting.DAL.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Domain.OAuth
{
    public class ClientStore : IClientStore
    {
        private readonly AppDbContext _ctx;

        public ClientStore(AppDbContext ctx)
        {
            _ctx = ctx;
        }
        public async Task<IOAuthClient> FindByClientIdAsync(string clientId)
        {
            IOAuthClient entity = (await _ctx.OAuthClients.FirstOrDefaultAsync(c => c.ClientId == clientId)) as IOAuthClient;
            return entity;
        }
    }
}
