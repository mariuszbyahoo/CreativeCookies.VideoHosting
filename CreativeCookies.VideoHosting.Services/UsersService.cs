using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.Services;
using CreativeCookies.VideoHosting.DTOs.Films;
using CreativeCookies.VideoHosting.DTOs.OAuth;
using Hangfire;

namespace CreativeCookies.VideoHosting.Services
{
    public class UsersService : IUsersService
    {
        private readonly IUsersRepository _repo;
        private readonly IBackgroundJobClient _hangfireJobClient;

        public UsersService(IUsersRepository repo, IBackgroundJobClient hangfireJobClient)
        {
            _repo = repo;
            _hangfireJobClient = hangfireJobClient;
        }

        public async Task<UsersPaginatedResultDto> GetUsersPaginatedResult(string search, int pageNumber, int pageSize, string role)
        {
            var paginatedResult = await _repo.GetUsersPaginatedResult(search, pageNumber, pageSize, role);
            return paginatedResult;
        }

        public bool AssignStripeCustomerId(string userId, string stripeCustomerId)
        {
            var res = _repo.AssignStripeCustomerId(userId, stripeCustomerId);
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

        public async Task<bool> DeleteBackgroundJobForUser(string userId)
        {
            var user = await _repo.GetUserById(userId);
            if(user == null) return false;
            else
            {
                var res = _hangfireJobClient.Delete(user.HangfireJobId);
                return res;
            }
        }

        public async Task<bool> ResetSubscriptionDates(string userId)
        {
            var user = await _repo.GetUserById(userId);
            if (user == null) return false;
            var res = _repo.ChangeSubscriptionDatesUTC(user.StripeCustomerId, DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)), DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)), false);
            return res;
        }

        public async Task<MyHubUserDto> GetUserById(string userId)
        {
            var user = await _repo.GetUserById(userId);
            return user;
        }
    }
}
