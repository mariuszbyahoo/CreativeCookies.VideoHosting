using CreativeCookies.VideoHosting.Contracts.DTOs.OAuth;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.DAL.DAOs.OAuth;
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

        public async Task<string> GetAuthorizationCode (string client_id, string userId, string redirect_uri, string code_challenge, string code_challenge_method)
        {
            var authorizationCode = AuthCodeGenerator.GenerateAuthorizationCode();
            var codeEntry = new AuthorizationCode()
            {
                ClientId = client_id,
                UserId = userId,
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
