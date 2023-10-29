using CreativeCookies.VideoHosting.DTOs.Films;
using CreativeCookies.VideoHosting.DTOs.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Services
{
    public interface IUsersService
    {
        Task<UsersPaginatedResultDto> GetUsersPaginatedResult(string search, int pageNumber, int pageSize, string role);

        Task<IList<MyHubUserDto>> GetAllUsers();
        
        bool HasUserAScheduledSubscription(string hangfireJobId);

        Task<MyHubUserDto> GetUserById(string userId);

        bool AssignStripeCustomerId(string userId, string stripeCustomerId);

        Task<bool> IsUserSubscriber(string userId);

        Task<SubscriptionDateRange> GetSubscriptionDates(string userId);

        /// <summary>
        /// Deletes the background hangfire job with scheduled order for a subscription
        /// </summary>
        /// <param name="userId">UserId to delete the background job for</param>
        /// <returns>true - operation succeed, otherwise false</returns>
        Task<bool> DeleteBackgroundJobForUser(string userId);

        Task<bool> ResetSubscriptionDates(string customerId);

        Task<MemoryStream> GenerateExcelFile(IEnumerable<MyHubUserDto> users);
    }
}
