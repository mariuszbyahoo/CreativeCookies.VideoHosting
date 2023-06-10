using CreativeCookies.VideoHosting.Contracts.DTOs.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Repositories.OAuth
{
    public interface IRefreshTokenRepository
    {
        Task<IRefreshToken> CreateRefreshToken(Guid userId);
        Task<IRefreshToken> FindRefreshToken(string refreshToken);
        Task<IMyHubUser> GetUserByRefreshToken(string? refresh_token);
        Task<bool> IsTokenValid(string refresh_token);
        Task RevokeRefreshToken(string refreshToken);
    }
}
