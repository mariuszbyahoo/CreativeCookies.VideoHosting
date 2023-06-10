using CreativeCookies.VideoHosting.Contracts.DTOs.OAuth;
using CreativeCookies.VideoHosting.Contracts.Repositories.OAuth;
using CreativeCookies.VideoHosting.Domain.DTOs.OAuth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Domain.Repositories.OAuth
{
    public class JWTRepository : IJWTRepository
    {
        private const int RefreshTokenLength = 32;

        public string GenerateAccessToken(Guid userId, string userEmail, Guid clientId, IConfiguration configuration, string issuer)
        {
            var secretKey = configuration["JWTSecretKey"];

            if(string.IsNullOrWhiteSpace(secretKey)) throw new ArgumentNullException(nameof(secretKey));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString().ToUpperInvariant()), 
                    new Claim("client_id", clientId.ToString().ToUpperInvariant()),
                    new Claim(ClaimTypes.Email, userEmail.ToUpperInvariant())
                }),
                Expires = DateTime.UtcNow.AddHours(1), 
                Issuer = issuer, 
                Audience = clientId.ToString().ToUpperInvariant(), 
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public IRefreshToken GenerateRefreshToken(Guid userId)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[RefreshTokenLength];

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                var byteBuffer = new byte[RefreshTokenLength];

                for (int i = 0; i < RefreshTokenLength; i++)
                {
                    rng.GetBytes(byteBuffer);
                    var randomIndex = byteBuffer[i] % chars.Length;
                    stringChars[i] = chars[randomIndex];
                }
            }

            var refreshToken = new RefreshTokenDto
            {
                Id = Guid.NewGuid(),
                Token = new string(stringChars),
                UserId = userId,
                CreationDate = DateTime.UtcNow,
                ExpirationDate = DateTime.UtcNow.AddHours(3),
            };

            return refreshToken;
        }
    }
}
