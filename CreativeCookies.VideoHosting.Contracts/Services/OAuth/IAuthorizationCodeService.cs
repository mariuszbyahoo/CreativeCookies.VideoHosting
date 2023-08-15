using CreativeCookies.VideoHosting.DTOs.OAuth;

namespace CreativeCookies.VideoHosting.Contracts.Services.OAuth
{
    public interface IAuthorizationCodeService
    {
        Task<string> GenerateAuthorizationCode(string client_id, string userId, string redirect_uri, string code_challenge, string code_challenge_method);
        Task ClearExpiredAuthorizationCodes();
        Task<MyHubUserDto> GetUserByAuthCodeAsync(string code);
    }
}
