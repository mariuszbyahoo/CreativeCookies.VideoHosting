using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.Services;
using CreativeCookies.VideoHosting.DTOs.Films;
using CreativeCookies.VideoHosting.DTOs.OAuth;
using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace CreativeCookies.VideoHosting.Services
{
    public class UsersService : IUsersService
    {
        private readonly IUsersRepository _repo;
        private readonly IBackgroundJobClient _hangfireJobClient;
        private readonly IMonitoringApi _monitoringApi;
        private readonly ILogger<UsersService> _logger;

        public UsersService(IUsersRepository repo, IBackgroundJobClient hangfireJobClient, IMonitoringApi monitoringApi, ILogger<UsersService> logger)
        {
            _repo = repo;
            _hangfireJobClient = hangfireJobClient;
            _monitoringApi = monitoringApi;
            _logger = logger;
        }

        public bool HasUserAScheduledSubscription(string hangfireJobId)
        {
            bool jobScheduledForFuture = false;

            var scheduledJobs = _monitoringApi.ScheduledJobs(0, int.MaxValue);
            var matchingJob = scheduledJobs.FirstOrDefault(j => j.Key == hangfireJobId);

            if (matchingJob.Value != null)
            {
                var enqueueAt = matchingJob.Value.EnqueueAt;
                if (enqueueAt > DateTime.UtcNow)
                {
                    jobScheduledForFuture = true;
                }
            }
            return jobScheduledForFuture;
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

        public async Task<IList<MyHubUserDto>> GetAllUsers()
        {
            var users = await _repo.GetAllUsers();
            return users;
        }

        public byte[]? GenerateExcelFile(IList<MyHubUserDto> users)
        {
            try
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Users");

                worksheet.Cells[1, 1].Value = "Email";
                worksheet.Cells[1, 2].Value = "UserRole";
                worksheet.Cells[1, 3].Value = "Stripe customer ID";
                worksheet.Cells[1, 4].Value = "Status";

                for (int i = 0; i < users.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = users[i].UserEmail;
                    worksheet.Cells[i + 2, 2].Value = users[i].Role;
                    worksheet.Cells[i + 2, 3].Value = users[i].StripeCustomerId;
                    worksheet.Cells[i + 2, 4].Value = users[i].IsActive ? "Email confirmed" : "Email is not confirmed";
                }

                return package.GetAsByteArray();
            } catch (Exception ex)
            {
                _logger.LogInformation("An exception occured inside of a GenerateExcelFile method");
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }
    }
}
