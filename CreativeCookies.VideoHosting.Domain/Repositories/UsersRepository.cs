using CreativeCookies.VideoHosting.Contracts.DTOs.OAuth;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.Domain.DTOs.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Domain.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public UsersRepository(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<IMyHubUser>> GetUsersList(string search, int pageNumber, int pageSize)
        {

            var users = _context.Users
                .Where(user => string.IsNullOrEmpty(search) || user.Email.Contains(search) || user.UserName.Contains(search))
                .OrderBy(user => user.Email)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new List<IMyHubUser>();

            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                result.Add(new MyHubUserDto(Guid.Parse(user.Id.ToUpperInvariant()), user.Email ?? user.UserName, string.Join(',', userRoles)));
            }
            return result;
        }
    }
}
