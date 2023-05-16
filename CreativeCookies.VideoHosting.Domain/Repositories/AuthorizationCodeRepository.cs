using CreativeCookies.VideoHosting.Contracts.DTOs.OAuth;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.DAL.DAOs.OAuth;
using CreativeCookies.VideoHosting.Domain.DTOs.OAuth;
using CreativeCookies.VideoHosting.Domain.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        /// <summary>
        /// Generates a new AuthorizationCode for particular user and particular client_id, and removes all auth codes issued to this user previousely.
        /// </summary>
        /// <param name="client_id"></param>
        /// <param name="userId"></param>
        /// <param name="redirect_uri"></param>
        /// <param name="code_challenge"></param>
        /// <param name="code_challenge_method"></param>
        /// <returns></returns>
        public async Task<string> GetAuthorizationCode(string client_id, string userId, string redirect_uri, string code_challenge, string code_challenge_method)
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
                Expiration = DateTime.UtcNow.AddMinutes(1)
            };
            var issuedAuthCodes = _ctx.AuthorizationCodes
                .Where(c =>
                    c.ClientId.ToLower().Equals(client_id.ToLower()) &&
                    c.UserId.ToLower().Equals(userId.ToLower()))
                .AsEnumerable();

            _ctx.RemoveRange(issuedAuthCodes);

            _ctx.AuthorizationCodes.Add(codeEntry);
            await _ctx.SaveChangesAsync();
            return authorizationCode;
        }

        public async Task<IMyHubUser> GetUserByAuthCodeAsync(string code)
        {
            var codeEntry = await _ctx.AuthorizationCodes.Where(c => c.Code.Equals(code)).FirstOrDefaultAsync();
            if (codeEntry == null) 
            { 
                return null; 
            }
            else
            {
                var user = await _ctx.Users.Where(u => u.Id.Equals(codeEntry.UserId))
                    .Select<IdentityUser, IMyHubUser>(r => new MyHubUser(Guid.Parse(r.Id), r.NormalizedEmail))
                    .FirstOrDefaultAsync();
                return user;
            }
        }
    }
}
