
using CreativeCookies.VideoHosting.DTOs.OAuth;

namespace CreativeCookies.VideoHosting.Contracts.Repositories
{
    public interface IAuthorizationCodeRepository
    {
        Task<string> SaveAuthorizationCode(string client_id, string userId, string redirect_uri, string code_challenge, string code_challenge_method, string authorizationCode);
        Task ClearExpiredAuthorizationCodes();
        Task<MyHubUserDto> GetUserByAuthCodeAsync(string code);
    }
}
