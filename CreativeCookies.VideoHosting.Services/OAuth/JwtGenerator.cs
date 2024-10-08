﻿using CreativeCookies.VideoHosting.Contracts.Services.OAuth;
using CreativeCookies.VideoHosting.DTOs.OAuth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CreativeCookies.VideoHosting.Services.OAuth
{
    public class JwtGenerator : IJWTGenerator
    {
        private const int RefreshTokenLength = 32;

        public string GenerateAccessToken(Guid userId, string userEmail, Guid clientId, IConfiguration configuration, string issuer, string userRole)
        {
            var secretKey = configuration["JWTSecretKey"];

            if (string.IsNullOrWhiteSpace(secretKey)) throw new ArgumentNullException(nameof(secretKey));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString().ToUpperInvariant()),
                    new Claim("client_id", clientId.ToString().ToUpperInvariant()),
                    new Claim(ClaimTypes.Email, userEmail.ToUpperInvariant()),
                    new Claim(ClaimTypes.Role, userRole)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = issuer,
                Audience = clientId.ToString().ToUpperInvariant(),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public RefreshTokenDto GenerateRefreshToken(Guid userId)
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

            var refreshToken = new RefreshTokenDto(Guid.NewGuid(), userId, new string(stringChars), DateTime.UtcNow, DateTime.UtcNow.AddHours(8));

            return refreshToken;
        }
    }
}
