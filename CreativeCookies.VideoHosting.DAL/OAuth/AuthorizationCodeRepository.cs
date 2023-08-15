using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.DAL.DAOs.OAuth;
using CreativeCookies.VideoHosting.DTOs.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DAL.OAuth
{
    public class AuthorizationCodeRepository : IAuthorizationCodeRepository
    {
        private readonly AppDbContext _ctx;
        private readonly ILogger<AuthorizationCodeRepository> _logger;
        public AuthorizationCodeRepository(AppDbContext ctx, ILogger<AuthorizationCodeRepository> logger)
        {
            _ctx = ctx;
            _logger = logger;
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
        /// Saves an AuthorizationCode for particular user and particular client_id to the database 
        /// </summary>
        /// <param name="client_id"></param>
        /// <param name="userId"></param>
        /// <param name="redirect_uri"></param>
        /// <param name="code_challenge"></param>
        /// <param name="code_challenge_method"></param>
        /// <returns></returns>
        public async Task<string> SaveAuthorizationCode(string client_id, string userId, string redirect_uri, string code_challenge, string code_challenge_method, string authorizationCode)
        {
            var codeEntry = new AuthorizationCode()
            {
                ClientId = client_id,
                UserId = userId,
                Code = authorizationCode,
                RedirectUri = redirect_uri,
                CodeChallenge = WebUtility.UrlDecode(code_challenge),
                CodeChallengeMethod = code_challenge_method,
                Expiration = DateTime.UtcNow.AddMinutes(1)
            };
            _logger.LogInformation($"code_challenge just before being saved to Dabatase : {codeEntry.CodeChallenge}");

            #region toAnotherMethod

            var issuedAuthCodes = _ctx.AuthorizationCodes
                .Where(c =>
                    c.ClientId.ToLower().Equals(client_id.ToLower()) &&
                    c.UserId.ToLower().Equals(userId.ToLower()))
                .AsEnumerable();

            _ctx.RemoveRange(issuedAuthCodes);
            #endregion

            _ctx.AuthorizationCodes.Add(codeEntry);
            await _ctx.SaveChangesAsync();
            return authorizationCode;
        }

        public async Task<MyHubUserDto> GetUserByAuthCodeAsync(string code)
        {
            var codeEntry = await _ctx.AuthorizationCodes.Where(c => c.Code.Equals(code)).FirstOrDefaultAsync();
            if (codeEntry == null)
            {
                return null;
            }
            else
            {
                var intermediateLookup = await _ctx.UserRoles.FirstOrDefaultAsync(r => r.UserId.Equals(codeEntry.UserId));
                var roleId = intermediateLookup.RoleId;
                var role = await _ctx.Roles.FirstOrDefaultAsync(r => r.Id.Equals(roleId));
                var user = await _ctx.Users.Where(u => u.Id.Equals(codeEntry.UserId))
                    .Select<IdentityUser, MyHubUserDto>(r => new MyHubUserDto(Guid.Parse(r.Id), r.NormalizedEmail, role.NormalizedName, r.EmailConfirmed))
                    .FirstOrDefaultAsync();
                return user;
            }
        }
    }
}
