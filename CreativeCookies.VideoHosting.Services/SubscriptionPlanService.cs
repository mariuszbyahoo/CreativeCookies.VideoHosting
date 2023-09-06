using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.Services.Stripe;
using CreativeCookies.VideoHosting.DTOs.Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Services
{
    public class SubscriptionPlanService : ISubscriptionPlanService
    {
        private readonly ISubscriptionPlanRepository _repo;

        public SubscriptionPlanService(ISubscriptionPlanRepository repo)
        {
            _repo = repo;
        }

        public async Task<int> DeleteSubscriptionPlan(string subscriptionPlanId)
        {
            var res = await _repo.DeleteSubscriptionPlan(subscriptionPlanId);
            return res;
        }

        public async Task<SubscriptionPlanDto> FetchSubscriptionPlanById(string subscriptionPlanId)
        {
            var res = await _repo.GetSubscriptionPlan(subscriptionPlanId);
            return res;
        }

        public async Task<SubscriptionPlanDto> FetchSubscriptionPlan()
        {
            var res = await _repo.GetAllSubscriptions();
            return res[0];
        }

        public async Task<bool> HasAnyProduct()
        {
            return await _repo.HasAnyProduct();
        }

        public async Task<SubscriptionPlanDto> SaveSubscriptionPlan(SubscriptionPlanDto subscriptionPlan)
        {
            var res = await _repo.CreateNewSubscriptionPlan(subscriptionPlan);
            return res;
        }

        public async Task<SubscriptionPlanDto> UpdateSubscriptionPlan(SubscriptionPlanDto subscriptionPlan)
        {
            var res = await _repo.UpdateSubscriptionPlan(subscriptionPlan);
            return res;
        }

        public async Task<SubscriptionPlanDto> UpsertSubscriptionPlan(SubscriptionPlanDto subscriptionPlan)
        {
            var existingEntity = await _repo.GetSubscriptionPlan(subscriptionPlan.Id);
            if (existingEntity == null)
                return await _repo.CreateNewSubscriptionPlan(subscriptionPlan);
            else
                return await _repo.UpdateSubscriptionPlan(subscriptionPlan);
        }
    }
}
