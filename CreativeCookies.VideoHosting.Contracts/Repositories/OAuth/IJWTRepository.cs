using CreativeCookies.VideoHosting.DTOs.OAuth;
using Microsoft.Extensions.Configuration;


namespace CreativeCookies.VideoHosting.Contracts.Repositories.OAuth
{
    public interface IJWTRepository
    {
        string GenerateAccessToken(Guid userId, string userEmail, Guid clientId, IConfiguration configuration, string issuer, string userRole);
        RefreshTokenDto GenerateRefreshToken(Guid userId);
    }
}
