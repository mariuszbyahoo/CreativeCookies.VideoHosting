using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.DAL.DAOs.OAuth;
using CreativeCookies.VideoHosting.Domain.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Domain.Repositories
{
    public class AuthorizationCodeRepository : IAuthorizationCodeRepository
    {
        private readonly AppDbContext _ctx;
        public AuthorizationCodeRepository(AppDbContext ctx) 
        {
            _ctx = ctx;
        }
        public async Task ClearExpiredAuthorizationCodes()
        {
            var expiredCodes = _ctx.AuthorizationCodes.Where(ac => ac.Expiration < DateTime.UtcNow).ToList();
            // HACK TODO: ADD LOGGER AND LOG DELETED Codes!
            // HACK TODO: Log eventual exceptions
            _ctx.AuthorizationCodes.RemoveRange(expiredCodes);
            await _ctx.SaveChangesAsync();
        }

        public async Task<string> GetAuthorizationCode(string client_id, string userId, string redirect_uri, string code_challenge, string code_challenge_method)
        {
            var authorizationCode = AuthCodeGenerator.GenerateAuthorizationCode();
            var codeEntry = new AuthorizationCode()
            {
                ClientId = client_id,
                UserId = userId,
                Code = authorizationCode,
                RedirectUri = redirect_uri,
                CodeChallenge = code_challenge, // HACK  to do with PKCE
                CodeChallengeMethod = code_challenge_method, // HACK  to do with PKCE
                Expiration = DateTime.UtcNow.AddMinutes(10)
            };

            _ctx.AuthorizationCodes.Add(codeEntry);
            await _ctx.SaveChangesAsync();
            return authorizationCode;
        }
    }
}
