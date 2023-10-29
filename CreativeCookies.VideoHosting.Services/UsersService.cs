using ClosedXML.Excel;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.Services;
using CreativeCookies.VideoHosting.DTOs.Films;
using CreativeCookies.VideoHosting.DTOs.OAuth;
using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;

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

        public async Task<MemoryStream> GenerateExcelFile(IEnumerable<MyHubUserDto> users)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Users");
            worksheet.Cell("A1").Value = "Email";
            worksheet.Cell("B1").Value = "Role";
            worksheet.Cell("C1").Value = "Is Active";
            worksheet.Cell("D1").Value = "Invoice Period Start Date UTC";
            worksheet.Cell("E1").Value = "Invoice Period End Date UTC";
            worksheet.Cell("F1").Value = "Stripe Customer ID";

            int row = 2;
            foreach (var user in users)
            {
                worksheet.Cell($"A{row}").Value = user.UserEmail;
                worksheet.Cell($"B{row}").Value = user.Role;
                worksheet.Cell($"C{row}").Value = user.IsActive ? "Yes" : "No";
                worksheet.Cell($"D{row}").Value = user.SubscriptionStartDateUTC;
                worksheet.Cell($"E{row}").Value = user.SubscriptionEndDateUTC;
                worksheet.Cell($"F{row}").Value = user.StripeCustomerId;
                row++;
            }

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }
    }
}
