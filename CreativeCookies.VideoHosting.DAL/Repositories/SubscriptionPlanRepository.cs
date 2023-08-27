using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.DAL.DAOs;
using CreativeCookies.VideoHosting.DTOs.Stripe;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DAL.Repositories
{
    public class SubscriptionPlanRepository : ISubscriptionPlanRepository
    {
        private readonly AppDbContext _ctx;

        public SubscriptionPlanRepository(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<SubscriptionPlanDto> SaveSubscriptionPlan(SubscriptionPlanDto newSubscriptionPlan)
        {
            var plan = new SubscriptionPlan() { 
                Description = newSubscriptionPlan.Description, 
                StripeProductId = newSubscriptionPlan.Id, 
                Name = newSubscriptionPlan.Name
            };
            await _ctx.SubscriptionPlans.AddAsync(plan);
            var res = await _ctx.SaveChangesAsync();
            if (res > 0)
            {
                return new SubscriptionPlanDto(plan.StripeProductId, plan.Name, plan.Description);
            }
            else return null;
        }

        public async Task<SubscriptionPlanDto> GetSubscriptionPlan(string productId)
        {
            var plan = await FetchDAOById(productId);
            if (plan == null) return null;
            return new SubscriptionPlanDto(plan.StripeProductId, plan.Name, plan.Description);
        }

        public async Task<int> DeleteSubscriptionPlan(string productId)
        {
            var plan = await FetchDAOById(productId);
            _ctx.SubscriptionPlans.Remove(plan);
            return await _ctx.SaveChangesAsync();
        }

        public async Task<SubscriptionPlanDto> UpdateSubscriptionPlan(SubscriptionPlanDto newPlanDto)
        {
            var dao = await FetchDAOById(newPlanDto.Id);
            dao.Description = newPlanDto.Description;
            dao.Name = newPlanDto.Name;
            _ctx.SubscriptionPlans.Update(dao);
            if (await _ctx.SaveChangesAsync() > 0) return newPlanDto;
            else return null;
        }

        private async Task<SubscriptionPlan> FetchDAOById(string productId)
        {
            return await _ctx.SubscriptionPlans.Where(p => p.StripeProductId.Equals(productId)).FirstOrDefaultAsync();
        }

        public async Task<IList<SubscriptionPlanDto>> GetAllSubscriptions()
        {
            return await _ctx.SubscriptionPlans.Select(p => new SubscriptionPlanDto(p.StripeProductId, p.Name, p.Description)).ToListAsync();
        }
    }
}
