using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.Services.OAuth;
using CreativeCookies.VideoHosting.Domain.OAuth;
using CreativeCookies.VideoHosting.DTOs.OAuth;

namespace CreativeCookies.VideoHosting.Services.OAuth
{
    public class AuthorizationCodeService : IAuthorizationCodeService
    {
        private readonly IAuthorizationCodeRepository _repo;

        public AuthorizationCodeService(IAuthorizationCodeRepository repo)
        {
            _repo = repo;
        }

        public async Task<string> GenerateAuthorizationCode(string client_id, string userId, string redirect_uri, string code_challenge, string code_challenge_method)
        {
            var authorizationCode = AuthCodeGenerator.GenerateAuthorizationCode();
            await _repo.DeletePreviousAuthCodesForUser(userId, client_id);
            await _repo.SaveAuthorizationCode(client_id, userId, redirect_uri, code_challenge, code_challenge_method, authorizationCode);
            return authorizationCode;
        }
        public async Task ClearExpiredAuthorizationCodes()
        {
            await _repo.ClearExpiredAuthorizationCodes();
        }
        public async Task<MyHubUserDto> GetUserByAuthCodeAsync(string code)
        {
            var result = await _repo.GetUserByAuthCodeAsync(code);
            return result;
        }
    }
}
