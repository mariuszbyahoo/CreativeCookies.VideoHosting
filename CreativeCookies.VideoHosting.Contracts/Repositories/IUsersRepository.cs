
using CreativeCookies.VideoHosting.DTOs.Films;
using CreativeCookies.VideoHosting.DTOs.OAuth;

namespace CreativeCookies.VideoHosting.Contracts.Repositories
{
    public interface IUsersRepository
    {
        Task<MyHubUserDto> GetUserByStripeCustomerId(string stripeCustomerId);

        Task<IList<MyHubUserDto>> GetAllUsers();

        MyHubUserDto? AssignHangfireJobIdToUser(string stripeCustomerId, string jobId);

        Task<MyHubUserDto> GetUserById(string userId);
        /// <summary>
        /// Changes the AspNetUser.SubscriptionEndDateUtc value
        /// </summary>
        /// <param name="customerId">Stripe customer Id of a user</param>
        /// <param name="endDateUtc">UTC subscription's end date</param>
        /// <returns>true - if operation succeeded, otherwise false</returns>
        bool ChangeSubscriptionEndDateUTC(string customerId, DateTime endDateUtc);

        /// <summary>
        /// Changes both of the AspNetUser.SubscriptionStartDateUTC and SubscriptionEndDateUTC
        /// </summary>
        /// <param name="customerId">Stripe customer Id of a user</param>
        /// <param name="startDateUtc">UTC subscription's start date</param>
        /// <param name="endDateUtc">UTC subscription's end date</param>
        /// <param name="addDelayForSubscriptions">This param controls is there a need to add 3 hours to specified endDateUtc</param>
        /// <returns>true - if operation succeeded, otherwise false</returns>
        bool ChangeSubscriptionDatesUTC(string customerId, DateTime startDateUtc, DateTime endDateUtc, bool addDelayForSubscriptions = true);

        Task<UsersPaginatedResultDto> GetUsersPaginatedResult(string search, int pageNumber, int pageSize, string role);

        /// <summary>
        /// Assigns stripeCustomerId to particular user in the database
        /// </summary>
        /// <param name="userId">Id of an user </param>
        /// <param name="stripeCustomerId">Stripe Customer Id to assign</param>
        /// <returns>True - success, False - error occured or no user has been found in the database</returns>
        bool AssignStripeCustomerId(string userId, string stripeCustomerId);

        /// <summary>
        /// Checks underlying DAL inf. and returns true if all is set as Subscriber
        /// </summary>
        /// <param name="userId">Id of a user to look up for</param>
        /// <returns>true - user successfuly granted with access, false - no or some error occured</returns>
        Task<bool> IsUserSubscriber(string userId);

        /// <summary>
        /// Returns date range of {StartDateUTC} - {EndDateUTC}
        /// </summary>
        /// <param name="userId">Id of a user to look up for</param>
        /// <returns>null, if no user has been found</returns>
        Task<SubscriptionDateRange> GetSubscriptionDates(string userId);
    }
}
