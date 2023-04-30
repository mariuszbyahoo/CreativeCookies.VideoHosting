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
        public async Task<bool> IsRedirectUriPresentInDatabase(string redirectUri)
        {
            var client = await _ctx.OAuthClients.FirstOrDefaultAsync(c => c.RedirectUri.Equals(redirectUri));
            if (client == null) return false;
            else return true;
        }

        public async Task<bool> WasAuthCodeIssued(string code, string client_id)
        {
            var entry = await _ctx.AuthorizationCodes.Where(c => c.Code.Equals(code)).FirstOrDefaultAsync();
            if (entry == null) return false;
            if (!entry.ClientId.Equals(client_id)) return false;
            return true;
        }

        public async Task<bool> WasRedirectUriRegisteredToClient(string redirect_uri, string client_id)
        {
            var entry = await _ctx.OAuthClients.FirstOrDefaultAsync(c => c.Id.ToString().Equals(client_id));
            if (entry == null) return false;
            else if (entry.RedirectUri.Equals(redirect_uri)) return true;
            else return false;
        }
    }
}
