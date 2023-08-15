using CreativeCookies.VideoHosting.DTOs.OAuth;
using Microsoft.Extensions.Configuration;


namespace CreativeCookies.VideoHosting.Contracts.Services.OAuth
{
    public interface IJWTGenerator
    {
        string GenerateAccessToken(Guid userId, string userEmail, Guid clientId, IConfiguration configuration, string issuer, string userRole);
        RefreshTokenDto GenerateRefreshToken(Guid userId);
    }
}
