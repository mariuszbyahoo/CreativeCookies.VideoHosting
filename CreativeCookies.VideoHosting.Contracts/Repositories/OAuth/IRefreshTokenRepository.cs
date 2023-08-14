using CreativeCookies.VideoHosting.DTOs.OAuth;

namespace CreativeCookies.VideoHosting.Contracts.Repositories.OAuth
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshTokenDto> CreateRefreshToken(Guid userId);
        Task<RefreshTokenDto> FindRefreshToken(string refreshToken);
        Task<MyHubUserDto> GetUserByRefreshToken(string? refresh_token);
        Task<bool> IsTokenValid(string refresh_token);
        Task RevokeRefreshToken(string refreshToken);
    }
}
