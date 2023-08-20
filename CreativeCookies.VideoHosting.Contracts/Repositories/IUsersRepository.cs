
using CreativeCookies.VideoHosting.DTOs.OAuth;

namespace CreativeCookies.VideoHosting.Contracts.Repositories
{
    public interface IUsersRepository
    {
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
