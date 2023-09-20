
using CreativeCookies.VideoHosting.DTOs.OAuth;

namespace CreativeCookies.VideoHosting.Contracts.Repositories
{
    public interface IUsersRepository
    {
        /// <summary>
        /// Changes the AspNetUser.SubscriptionEndDateUtc value
        /// </summary>
        /// <param name="customerId">Stripe customer Id of a user</param>
        /// <param name="endDateUtc">UTC subscription's end date</param>
        /// <returns>true - if operation succeeded, otherwise false</returns>
        Task<bool> ChangeSubscriptionEndDateUTC(string customerId, DateTime endDateUtc);

        Task<UsersPaginatedResultDto> GetUsersPaginatedResult(string search, int pageNumber, int pageSize, string role);

        /// <summary>
        /// Assigns stripeCustomerId to particular user in the database
        /// </summary>
        /// <param name="userId">Id of an user </param>
        /// <param name="stripeCustomerId">Stripe Customer Id to assign</param>
        /// <returns>True - success, False - error occured or no user has been found in the database</returns>
        Task<bool> AssignStripeCustomerId(string userId, string stripeCustomerId);
    }
}
