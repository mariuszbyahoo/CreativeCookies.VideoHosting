using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.Services.IdP;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.DAL.DAOs.OAuth;
using CreativeCookies.VideoHosting.DTOs;
using CreativeCookies.VideoHosting.DTOs.Films;
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

        public async Task<MyHubUserDto> GetUserById(string userId)
        {
            MyHubUserDto dto;
            var loweredUserId = userId.ToLower();
            var dao = await _context.Users.Include(dao => dao.Address).Where(u => u.Id.Equals(loweredUserId)).FirstOrDefaultAsync();
            if (dao == null)
            {
                return null;
            }
            if (dao.Address == null)
            {
                dto = new MyHubUserDto(Guid.Parse(dao.Id), dao.Email, string.Empty, dao.EmailConfirmed, dao.StripeCustomerId, dao.SubscriptionStartDateUTC, dao.SubscriptionEndDateUTC, dao.HangfireJobId);
            }
            else
            {
                var addressDto = new InvoiceAddressDto(dao.Address.Id, dao.Address.FirstName, dao.Address.LastName, dao.Address.Street, dao.Address.HouseNo, dao.Address.AppartmentNo, dao.Address.PostCode, dao.Address.City, dao.Address.Country, dao.Address.UserId);
                dto = new MyHubUserDto(Guid.Parse(dao.Id), dao.Email, string.Empty, dao.EmailConfirmed, dao.StripeCustomerId, dao.SubscriptionStartDateUTC, dao.SubscriptionEndDateUTC, dao.HangfireJobId, addressDto);
            }
            dto.Role = string.Join(",", await _userManager.GetRolesAsync(dto));
            return dto;
        }

        public async Task<MyHubUserDto> GetUserByStripeCustomerId(string stripeCustomerId)
        {
            MyHubUserDto dto;
            var dao = await _context.Users.Include(dao => dao.Address).Where(u => u.StripeCustomerId.Equals(stripeCustomerId)).FirstOrDefaultAsync();
            dto =  new MyHubUserDto(Guid.Parse(dao.Id), dao.Email, string.Empty, dao.EmailConfirmed, dao.StripeCustomerId, dao.SubscriptionStartDateUTC, dao.SubscriptionEndDateUTC, dao.HangfireJobId);
            if (dao.Address == null)
            {
                dto = new MyHubUserDto(Guid.Parse(dao.Id), dao.Email, string.Empty, dao.EmailConfirmed, dao.StripeCustomerId, dao.SubscriptionStartDateUTC, dao.SubscriptionEndDateUTC, dao.HangfireJobId);
            }
            else
            {
                var addressDto = new InvoiceAddressDto(dao.Address.Id, dao.Address.FirstName, dao.Address.LastName, dao.Address.Street, dao.Address.HouseNo, dao.Address.AppartmentNo, dao.Address.PostCode, dao.Address.City, dao.Address.Country, dao.Address.UserId);
                dto = new MyHubUserDto(Guid.Parse(dao.Id), dao.Email, string.Empty, dao.EmailConfirmed, dao.StripeCustomerId, dao.SubscriptionStartDateUTC, dao.SubscriptionEndDateUTC, dao.HangfireJobId, addressDto);
            }
            dto.Role = string.Join(",", await _userManager.GetRolesAsync(dto));
            return dto;
        }

        public MyHubUserDto? AssignHangfireJobIdToUser(string stripeCustomerId, string jobId)
        {
            var dao = _context.Users.Where(u => u.StripeCustomerId.Equals(stripeCustomerId)).FirstOrDefault();
            dao.HangfireJobId = jobId;
            var result = _context.SaveChanges();
            if (result > 0)
            {
                var dto = new MyHubUserDto(Guid.Parse(dao.Id), dao.Email, string.Empty, dao.EmailConfirmed, dao.StripeCustomerId, dao.SubscriptionStartDateUTC, dao.SubscriptionEndDateUTC, dao.HangfireJobId);
                return dto;
            }
            return null;
        }

        public bool ChangeSubscriptionEndDateUTC(string customerId, DateTime endDateUtc)
        {
            var dao = _context.Users.Where(u => u.StripeCustomerId.Equals(customerId)).FirstOrDefault();
            dao.SubscriptionEndDateUTC = endDateUtc;
            var result = _context.SaveChanges();
            return result > 0;
        }

        public bool ChangeSubscriptionDatesUTC(string customerId, DateTime startDateUtc, DateTime endDateUtc, bool addDelayForSubscriptions = true)
        {
            var dao = _context.Users.Where(u => u.StripeCustomerId.Equals(customerId)).FirstOrDefault();
            dao.SubscriptionStartDateUTC = startDateUtc;
            dao.SubscriptionEndDateUTC = addDelayForSubscriptions ? endDateUtc + TimeSpan.FromHours(3) : endDateUtc;
            var result = _context.SaveChanges();
            return result > 0;
        }

        public bool AssignStripeCustomerId(string userId, string stripeCustomerId)
        {
            var dao = _context.Users.Where(u => u.Id.Equals(userId.ToString())).FirstOrDefault();
            if (dao == null) return false;
            dao.StripeCustomerId = stripeCustomerId;
            _context.SaveChanges();
            return true;
        }

        public async Task<UsersPaginatedResultDto> GetUsersPaginatedResult(string search, int pageNumber, int pageSize, string role)
        {
            var usersQuery = _context.Users.Include(user => user.Address).Where(user => string.IsNullOrEmpty(search) || user.Email.Contains(search) || user.UserName.Contains(search));
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
                
                if(user.Address != null)
                {
                    dto.Address = new InvoiceAddressDto(user.Address.Id, user.Address.FirstName, user.Address.LastName, user.Address.Street, user.Address.HouseNo, user.Address.AppartmentNo, user.Address.PostCode, user.Address.City, user.Address.Country, user.Address.UserId);
                }

                if (toAdd) result.Add(dto);

            }
            return new UsersPaginatedResultDto(result, usersCount > result.Count(), pageNumber, totalPages);
        }

        public async Task<bool> IsUserSubscriber(string userId)
        {
            var result = await _context.Users.Where(u => u.Id.ToLower() == userId.ToLower()).FirstOrDefaultAsync();

            return result != null && result.SubscriptionStartDateUTC < DateTime.UtcNow && result.SubscriptionEndDateUTC > DateTime.UtcNow;
        }

        public async Task<SubscriptionDateRange> GetSubscriptionDates(string userId)
        {
            var result = await _context.Users.Where(u => u.Id.ToLower() == userId.ToLower()).FirstOrDefaultAsync();
            if (result == null) return null;
            return new SubscriptionDateRange(result.SubscriptionStartDateUTC, result.SubscriptionEndDateUTC);
        }

        public async Task<IList<MyHubUserDto>> GetAllUsers()
        {
            var result = new List<MyHubUserDto>();
            var userDAOs = _context.Users;
            foreach (var dao in userDAOs)
            {
                var user = await _userManager.FindByIdAsync(dao.Id);
                if (dao.Address != null)
                {
                    user.Address = new InvoiceAddressDto(user.Address.Id, user.Address.FirstName, user.Address.LastName, user.Address.Street, user.Address.HouseNo, user.Address.AppartmentNo, user.Address.PostCode, user.Address.City, user.Address.Country, user.Address.UserId);
                }
                result.Add(user);
            }
            return result;
        }
    }
}