﻿using CreativeCookies.VideoHosting.Contracts.Repositories.OAuth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Domain.Repositories.OAuth
{
    public class JWTRepository : IJWTRepository
    {

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
                Expires = DateTime.UtcNow.AddMinutes(60), 
                Issuer = issuer, 
                Audience = clientId.ToString().ToUpperInvariant(), 
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
