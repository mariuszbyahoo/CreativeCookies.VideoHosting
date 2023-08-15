using CreativeCookies.VideoHosting.DTOs.OAuth;

namespace CreativeCookies.VideoHosting.Contracts.Repositories.OAuth
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshTokenDto> SaveRefreshToken(RefreshTokenDto refreshTokenDto);
        Task<RefreshTokenDto[]> GetRefreshTokens(Guid userId);
        Task<MyHubUserDto> GetUserByRefreshToken(string? refresh_token);
        Task<bool> IsTokenValid(string refresh_token);
        Task DeleteRefreshToken(RefreshTokenDto refreshToken);
        Task DeleteRefreshTokens(params RefreshTokenDto[] refreshTokens);
        Task RevokeRefreshToken(string refreshToken);
    }
}
    