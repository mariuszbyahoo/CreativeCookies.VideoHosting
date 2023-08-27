using CreativeCookies.VideoHosting.DTOs.Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Services.Stripe
{
    public interface ISubscriptionPlanService
    {
        /// <summary>
        /// Gets subscription plan DTO by it's Stripe Product Id
        /// </summary>
        /// <param name="subscriptionPlanId">Id to fetch by</param>
        /// <returns>SubscriptionPlanDto, or null if none found</returns>
        Task<SubscriptionPlanDto> FetchSubscriptionPlanById(string subscriptionPlanId);
        /// <summary>
        /// Retrieves all of the Subscription plans existing in the database
        /// </summary>
        /// <returns>IList of susbscriptionPlanDto</returns>
        Task<IList<SubscriptionPlanDto>> FetchSubscriptionPlans();
        /// <summary>
        /// Updates existing subscription plan in the database and returns subscriptionPlanDto
        /// </summary>
        /// <param name="subscriptionPlan">DTO containing new values to save</param>
        /// <returns>If succeeds, returns SubscriptionPlanDto, if failure occurs - returns null</returns>
        Task<SubscriptionPlanDto> UpdateSubscriptionPlan(SubscriptionPlanDto subscriptionPlan);
        /// <summary>
        /// Checks, if subscription plan exist, and if so - updates existing one, if not - creates a new one
        /// </summary>
        /// <param name="subscriptionPlan">Dto containing values to upsert</param>
        /// <returns>If operation succeeded - Dto containing saved values, if not - null</returns>
        Task<SubscriptionPlanDto> UpsertSubscriptionPlan(SubscriptionPlanDto subscriptionPlan);
        /// <summary>
        /// Deletes existing Subscription Plan record from the database
        /// </summary>
        /// <param name="subscriptionPlanId">Stripe Product's ID to delete</param>
        /// <returns>int value indicating of how many entities has been removed from the database</returns>
        Task<int> DeleteSubscriptionPlan(string subscriptionPlanId);
        /// <summary>
        /// Saves new subscription plan to the database
        /// </summary>
        /// <param name="subscriptionPlan">Subscription plan to save</param>
        /// <returns>If operation succeeds - returns SubscriptionPlanDto, if failure occured - returns null</returns>
        Task<SubscriptionPlanDto> SaveSubscriptionPlan(SubscriptionPlanDto subscriptionPlan);
    }
}
