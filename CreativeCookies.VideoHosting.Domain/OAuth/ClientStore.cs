using CreativeCookies.VideoHosting.Contracts.DTOs.OAuth;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.DAL.DAOs.OAuth;
using CreativeCookies.VideoHosting.Domain.OAuth.DTOs;
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
        public async Task<IOAuthClient> FindByClientIdAsync(Guid clientId)
        {
            var client = await _ctx.OAuthClients
                        .Include(c => c.AllowedScopes)
                        .FirstOrDefaultAsync(c => c.Id.Equals(clientId));
            if (client == null)
            {
                return null;
            }
            else
            {
                var clientDto = new OAuthClientDto
                {
                    ClientSecret = client.ClientSecret,
                    RedirectUri = client.RedirectUri,
                    AllowedScopes = client.AllowedScopes.Select(scope => new AllowedScopeDto 
                    { 
                        Id = scope.Id,
                        OAuthClientId = scope.OAuthClientId,
                        Scope = scope.Scope
                    }).Cast<IAllowedScope>().ToList(),
                    Id = client.Id
                };
                return clientDto;
            }
        }
    }
}
