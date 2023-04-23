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
        public async Task<IOAuthClient> FindByClientIdAsync(string clientId)
        {
            var client = await _ctx.OAuthClients
                        .Include(c => c.AllowedScopes)
                        .FirstOrDefaultAsync(c => c.ClientId.Equals(clientId));
            if (client == null)
            {
                return null;
            }
            else
            {
                var clientDto = new OAuthClientDto
                {
                    ClientId = client.ClientId,
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

        public async Task<string> GetAuthorizationCode (string client_id, string userId, string redirect_uri, string code_challenge, string code_challenge_method)
        {
            var authorizationCode = AuthCodeGenerator.GenerateAuthorizationCode();
            var codeEntry = new AuthorizationCode()
            {
                ClientId = client_id,
                UserId = userId,
                Code = authorizationCode,
                RedirectUri = redirect_uri,
                CodeChallenge = code_challenge,
                CodeChallengeMethod = code_challenge_method,
                Expiration = DateTime.UtcNow.AddMinutes(10)
            };

            _ctx.AuthorizationCodes.Add(codeEntry);
            await _ctx.SaveChangesAsync();
            return authorizationCode;
        }
    }
}
