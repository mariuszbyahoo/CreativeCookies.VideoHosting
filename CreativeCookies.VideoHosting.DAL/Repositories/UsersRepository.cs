using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.Services.IdP;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.DAL.DAOs.OAuth;
using CreativeCookies.VideoHosting.DTOs.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CreativeCookies.VideoHosting.DAL.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private readonly AppDbContext _context;
        private readonly IMyHubUserManager _userManager;

        public UsersRepository(AppDbContext context, IMyHubUserManager userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<MyHubUserDto> GetUserByStripeCustomerId(string stripeCustomerId)
        {
            var dao = await _context.Users.Where(u => u.StripeCustomerId.Equals(stripeCustomerId)).FirstOrDefaultAsync();
            var dto =  new MyHubUserDto(Guid.Parse(dao.Id), dao.Email, string.Empty, dao.EmailConfirmed, dao.StripeCustomerId, dao.SubscriptionEndDateUTC);
            dto.Role = string.Join(",",await _userManager.GetRolesAsync(dto));
            return dto;
        }

        public async Task<bool> ChangeSubscriptionEndDateUTC(string customerId, DateTime endDateUtc)
        {
            var dao = await _context.Users.Where(u => u.StripeCustomerId.Equals(customerId)).FirstOrDefaultAsync();
            dao.SubscriptionEndDateUTC = endDateUtc;
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> AssignStripeCustomerId(string userId, string stripeCustomerId)
        {
            var dao = await _context.Users.Where(u => u.Id.Equals(userId.ToString())).FirstOrDefaultAsync();
            if (dao == null) return false;
            dao.StripeCustomerId = stripeCustomerId;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UsersPaginatedResultDto> GetUsersPaginatedResult(string search, int pageNumber, int pageSize, string role)
        {
            var usersQuery = _context.Users.Where(user => string.IsNullOrEmpty(search) || user.Email.Contains(search) || user.UserName.Contains(search));
            double usersCount = await usersQuery.CountAsync();
            var users = usersQuery
                .OrderBy(user => user.Email)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            var hasMore = users.Count > usersCount;
            int totalPages = int.Parse(Math.Ceiling(usersCount / pageSize).ToString());
            var result = new List<MyHubUserDto>();

            foreach (var user in users)
            {
                var toAdd = false;
                var dto = await _userManager.FindByIdAsync(user.Id);
                var userRoles = await _userManager.GetRolesAsync(dto);
                var matchingRole = userRoles.FirstOrDefault(r => r.ToLowerInvariant() == role.ToLowerInvariant());
                if (role.Equals("any", StringComparison.InvariantCultureIgnoreCase))
                {
                    toAdd = true;
                    matchingRole = userRoles.First();
                }
                else if (!string.IsNullOrWhiteSpace(matchingRole)) toAdd = true;

                if (toAdd) result.Add(dto);

            }
            return new UsersPaginatedResultDto(result, usersCount > result.Count(), pageNumber, totalPages);
        }

        public async Task<bool> IsUserSubscriber(string userId)
        {
            var result = await _context.Users.Where(u => u.Id.ToLower() == userId.ToLower()).FirstOrDefaultAsync();

            return result != null && result.SubscriptionEndDateUTC > DateTime.UtcNow;
        }
    }
}