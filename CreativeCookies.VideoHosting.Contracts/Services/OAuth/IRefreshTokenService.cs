using CreativeCookies.VideoHosting.DTOs.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Services.OAuth
{
    public interface IRefreshTokenService
    {
        Task<RefreshTokenDto> GetNewRefreshToken(Guid userId);
        Task<MyHubUserDto> GetUserByRefreshToken(string? refresh_token);
        Task<bool> IsTokenValid(string refresh_token);
        Task RevokeRefreshToken(string refreshToken);
    }
}
