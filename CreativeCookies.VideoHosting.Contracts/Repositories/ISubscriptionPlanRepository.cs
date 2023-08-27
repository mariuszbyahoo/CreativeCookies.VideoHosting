using CreativeCookies.VideoHosting.DTOs.Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Repositories
{
    public interface ISubscriptionPlanRepository
    {
        /// <summary>
        /// Saves new subscription plan to the database
        /// </summary>
        /// <param name="newSubscriptionPlan">Subscription plan to save</param>
        /// <returns>If operation succeeds - returns SubscriptionPlanDto, if failure occured - returns null</returns>
        Task<SubscriptionPlanDto> SaveSubscriptionPlan(SubscriptionPlanDto newSubscriptionPlan);

        /// <summary>
        /// Gets subscription plan DTO by it's Stripe Product Id
        /// </summary>
        /// <param name="productId">Id to fetch by</param>
        /// <returns>SubscriptionPlanDto, or null if none found</returns>
        Task<SubscriptionPlanDto> GetSubscriptionPlan(string productId);

        /// <summary>
        /// Retrieves all of the Subscription plans existing in the database
        /// </summary>
        /// <returns>IList of susbscriptionPlanDto</returns>
        Task<IList<SubscriptionPlanDto>> GetAllSubscriptions();

        /// <summary>
        /// Deletes existing Subscription Plan record from the database
        /// </summary>
        /// <param name="productId">Stripe Product's ID to delete</param>
        /// <returns>int value indicating of how many entities has been removed from the database</returns>
        Task<int> DeleteSubscriptionPlan(string productId);

        /// <summary>
        /// Updates existing subscription plan in the database and returns subscriptionPlanDto
        /// </summary>
        /// <param name="dto">DTO containing new values to save</param>
        /// <returns>If succeeds, returns SubscriptionPlanDto, if failure occurs - returns null</returns>
        Task<SubscriptionPlanDto> UpdateSubscriptionPlan(SubscriptionPlanDto dto);
    }
}
