using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.Services;
using CreativeCookies.VideoHosting.DTOs.Films;
using CreativeCookies.VideoHosting.DTOs.OAuth;

namespace CreativeCookies.VideoHosting.Services
{
    public class UsersService : IUsersService
    {
        private readonly IUsersRepository _repo;

        public UsersService(IUsersRepository repo)
        {
            _repo = repo;
        }

        public async Task<UsersPaginatedResultDto> GetUsersPaginatedResult(string search, int pageNumber, int pageSize, string role)
        {
            var paginatedResult = await _repo.GetUsersPaginatedResult(search, pageNumber, pageSize, role);
            return paginatedResult;
        }

        public async Task<bool> AssignStripeCustomerId(string userId, string stripeCustomerId)
        {
            var res = await _repo.AssignStripeCustomerId(userId, stripeCustomerId);
            return res;
        }

        public async Task<bool> IsUserSubscriber(string userId)
        {
            var res = await _repo.IsUserSubscriber(userId);
            return res;
        }

        public async Task<SubscriptionDateRange> GetSubscriptionDates(string userId)
        {
            var res = await _repo.GetSubscriptionDates(userId);
            return res;
        }
    }
}
