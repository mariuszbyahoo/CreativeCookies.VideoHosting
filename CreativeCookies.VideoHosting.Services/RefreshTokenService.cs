using CreativeCookies.VideoHosting.Contracts.Repositories.OAuth;
using CreativeCookies.VideoHosting.Contracts.Services.OAuth;
using CreativeCookies.VideoHosting.DTOs.OAuth;

namespace CreativeCookies.VideoHosting.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IJWTGenerator _jwtGenerator;

        public RefreshTokenService(IRefreshTokenRepository refreshTokenRepository, IJWTGenerator jwtGenerator)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _jwtGenerator = jwtGenerator;
        }

        public async Task<RefreshTokenDto> GetNewRefreshToken(Guid userId)
        {
            var existingRefreshTokens = await _refreshTokenRepository.GetRefreshTokens(userId);
            if(existingRefreshTokens != null && existingRefreshTokens.Length > 0)
            {
                if (existingRefreshTokens.Length > 1)
                {
                    await _refreshTokenRepository.DeleteRefreshTokens(existingRefreshTokens);
                }
                else
                {
                    await _refreshTokenRepository.DeleteRefreshToken(existingRefreshTokens.First());
                }
            }
            var refreshToken = _jwtGenerator.GenerateRefreshToken(userId);
            await _refreshTokenRepository.SaveRefreshToken(refreshToken);
            return refreshToken;
        }

        public async Task<MyHubUserDto> GetUserByRefreshToken(string? refresh_token)
        {
            var res = await _refreshTokenRepository.GetUserByRefreshToken(refresh_token);
            return res;
        }

        public async Task<bool> IsTokenValid(string refresh_token)
        {
            var res = await _refreshTokenRepository.IsTokenValid(refresh_token);
            return res;
        }

        public async Task RevokeRefreshToken(string refreshToken)
        {
            await _refreshTokenRepository.RevokeRefreshToken(refreshToken);
        }
    }
}
