using CreativeCookies.VideoHosting.Contracts.Repositories.OAuth;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.DAL.DAOs.OAuth;
using CreativeCookies.VideoHosting.DTOs.OAuth;
using Microsoft.EntityFrameworkCore;

namespace CreativeCookies.VideoHosting.DAL.Repositories.OAuth
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _context;

        public RefreshTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RefreshTokenDto[]> GetRefreshTokens(Guid userId)
        {
            var issuedTokens = await _context.RefreshTokens
                .Where(t => t.UserId.Equals(userId.ToString()))
                .Select(t => new RefreshTokenDto(t.Id, Guid.Parse(t.UserId), t.Token, t.CreatedAt, t.Expires))
                .ToArrayAsync();
            return issuedTokens;
        }

        public async Task DeleteRefreshToken(RefreshTokenDto refreshToken)
        {
            var dao = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Id.Equals(refreshToken.Id));

            if (dao != null)
            {
                _context.RefreshTokens.Remove(dao);

                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteRefreshTokens(params RefreshTokenDto[] refreshTokens)
        {
            _context.RemoveRange(refreshTokens);

            await _context.SaveChangesAsync();
        }

        public async Task<RefreshTokenDto> SaveRefreshToken(RefreshTokenDto refreshTokenDto)
        {
            var refreshTokenDao = new RefreshTokenDAO
            {
                UserId = refreshTokenDto.UserId.ToString(),
                Token = refreshTokenDto.Token,
                CreatedAt = refreshTokenDto.CreationDate,
                Expires = refreshTokenDto.ExpirationDate,
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshTokenDao);
            await _context.SaveChangesAsync();

            return new RefreshTokenDto(refreshTokenDao.Id, Guid.Parse(refreshTokenDao.UserId), refreshTokenDao.Token, refreshTokenDao.CreatedAt, refreshTokenDao.Expires);
        }

        public async Task<bool> IsTokenValid(string refresh_token)
        {
            var tokenEntry = await _context.RefreshTokens.Where(t => t.Token.Equals(refresh_token)).FirstOrDefaultAsync();
            if (tokenEntry != null && !tokenEntry.IsRevoked && tokenEntry.Expires > DateTime.UtcNow) return true;
            else return false;
        }

        public async Task<MyHubUserDto> GetUserByRefreshToken(string? refresh_token)
        {
            var tokenEntry = await _context.RefreshTokens.Where(t => t.Token.Equals(refresh_token)).FirstOrDefaultAsync();
            if (tokenEntry == null) return null;
            else
            {
                var user = await _context.Users.Where(u => u.Id.Equals(tokenEntry.UserId)).FirstOrDefaultAsync();
                var intermediateLookup = await _context.UserRoles.FirstOrDefaultAsync(r => r.UserId.Equals(tokenEntry.UserId));
                var roleId = intermediateLookup.RoleId;
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id.Equals(roleId));

                return new MyHubUserDto(Guid.Parse(tokenEntry.UserId), user.NormalizedEmail, role.NormalizedName, user.EmailConfirmed);
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
