using CreativeCookies.VideoHosting.Contracts.Repositories;
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
        private readonly UserManager<MyHubUser> _userManager;

        public UsersRepository(AppDbContext context, UserManager<MyHubUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
                var userRoles = await _userManager.GetRolesAsync(user);
                var matchingRole = userRoles.FirstOrDefault(r => r.ToLowerInvariant() == role.ToLowerInvariant());
                if (role.Equals("any", StringComparison.InvariantCultureIgnoreCase))
                {
                    toAdd = true;
                    matchingRole = userRoles.First();
                }
                else if (!string.IsNullOrWhiteSpace(matchingRole)) toAdd = true;

                if (toAdd) result.Add(new MyHubUserDto(Guid.Parse(user.Id.ToUpperInvariant()), user.Email ?? user.UserName, matchingRole, user.EmailConfirmed, user.StripeCustomerId));

            }
            return new UsersPaginatedResultDto(result, usersCount > result.Count(), pageNumber, totalPages);
        }
    }
}
