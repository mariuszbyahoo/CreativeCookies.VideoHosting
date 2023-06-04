﻿using CreativeCookies.VideoHosting.Contracts.DTOs.OAuth;
using CreativeCookies.VideoHosting.Contracts.Repositories.OAuth;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.DAL.DAOs.OAuth;
using CreativeCookies.VideoHosting.Domain.DTOs.OAuth;
using CreativeCookies.VideoHosting.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Domain.Repositories.OAuth
{

    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _context;
        private readonly IJWTRepository _jwtRepository;

        public RefreshTokenRepository(AppDbContext context, IJWTRepository jwtRepository)
        {
            _context = context;
            _jwtRepository = jwtRepository;
        }

        public async Task<IRefreshToken> CreateRefreshToken(Guid userId)
        {
            var refreshTokenDto = _jwtRepository.GenerateRefreshToken(userId);

            var refreshTokenDao = new RefreshTokenDAO
            {
                UserId = refreshTokenDto.UserId.ToString().ToUpperInvariant(),
                Token = refreshTokenDto.Token,
                CreatedAt = refreshTokenDto.CreationDate,
                Expires = refreshTokenDto.ExpirationDate,
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshTokenDao);
            await _context.SaveChangesAsync();

            return new RefreshToken
            {
                Id = refreshTokenDao.Id,
                Token = refreshTokenDao.Token,
                CreationDate = refreshTokenDao.CreatedAt,
                ExpirationDate = refreshTokenDao.Expires,
                UserId = Guid.Parse(refreshTokenDao.UserId)
            };
        }

        public async Task<IRefreshToken> FindRefreshToken(string token)
        {
            var refreshTokenDAO = (await _context.RefreshTokens.ToListAsync()).FirstOrDefault(rt => rt.Token.Equals(token, StringComparison.InvariantCultureIgnoreCase));

            if (refreshTokenDAO == null || refreshTokenDAO.IsRevoked)
            {
                return null;
            }

            return new RefreshToken
            {
                Id = refreshTokenDAO.Id,
                Token = refreshTokenDAO.Token,
                CreationDate = refreshTokenDAO.CreatedAt,
                ExpirationDate = refreshTokenDAO.Expires,
                UserId = Guid.Parse(refreshTokenDAO.UserId)
            };
        }

        public async Task<IMyHubUser> GetUserByRefreshToken(string? refresh_token)
        {
            var tokenEntry = await _context.RefreshTokens.Where(t => t.Token.Equals(refresh_token)).FirstOrDefaultAsync();
            if (tokenEntry == null) return null;
            else
            {
                var user = await _context.Users.Where(u => u.Id.ToUpperInvariant().Equals(tokenEntry.Id.ToString().ToUpperInvariant())).FirstOrDefaultAsync();
                return new MyHubUserDto(Guid.Parse(tokenEntry.UserId), user.NormalizedEmail);
            }
        }

        public async Task RevokeRefreshToken(string token)
        {
            var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);

            if (refreshToken != null)
            {
                refreshToken.IsRevoked = true;
                _context.RefreshTokens.Update(refreshToken);
                await _context.SaveChangesAsync();
            }
        }
    }
}
