﻿using CreativeCookies.VideoHosting.API.DTOs;
using CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe;
using CreativeCookies.VideoHosting.Contracts.Services.Stripe;
using CreativeCookies.VideoHosting.DTOs.Stripe;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Stripe;
using System.Runtime.CompilerServices;

namespace CreativeCookies.VideoHosting.API.Controllers
{
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ADMIN,admin")]
    [ApiController]
    public class StripeProductsController : ControllerBase
    {
        private readonly IStripeProductsService _stripeProductsService;
        private readonly ISubscriptionPlanService _subscriptionPlanService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripeProductsController> _logger;

        public StripeProductsController(IStripeProductsService stripeProductsService, ISubscriptionPlanService subscriptionPlanService, IConfiguration configuration, ILogger<StripeProductsController> logger)
        {
            _stripeProductsService = stripeProductsService;
            _subscriptionPlanService = subscriptionPlanService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("HasAnyProduct")]
        public async Task<ActionResult<bool>> HasAnyProduct()
        {
            return await _subscriptionPlanService.HasAnyProduct();
        }

        [HttpPost("UpsertSubscriptionPlan")]
        public async Task<ActionResult<SubscriptionPlanDto>> UpsertSubscriptionPlan([FromBody]StripeProductCreationDto model)
        {
            if (string.IsNullOrEmpty(model.Name)) return BadRequest("Name cannot be empty string");
            if (string.IsNullOrWhiteSpace(model.Description)) return BadRequest("Description cannot be an empty string");
            var res = await _stripeProductsService.UpsertStripeProduct(model.Name, model.Description);
            return Ok(res);
        }

        [HttpPost("CreateStripePrice")]
        public async Task<ActionResult<PriceDto>> CreateStripePrice(string stripeProductId, string currencyCode, int unitAmount)
        {
            if (string.IsNullOrWhiteSpace(stripeProductId)) return BadRequest($"StripeProductId cannot be empty");
            if (unitAmount <= 0) return BadRequest($"Amount has to be greater than 0");
            if (string.IsNullOrWhiteSpace(currencyCode) || currencyCode.Length != 3) return BadRequest($"Currency Code has to be three letter long : https://stripe.com/docs/currencies");
            var res = await _stripeProductsService.CreateStripePrice(stripeProductId, currencyCode, unitAmount);
            return Ok(res);
        }

        [HttpPost("DeactivateStripePrice")]
        public async Task<ActionResult<PriceDto>> DeactivateStripePrice(string priceId)
        {
            if (string.IsNullOrWhiteSpace(priceId)) return BadRequest("PriceId cannot be empty");
            var res = await _stripeProductsService.DeactivateStripePrice(priceId);
            return Ok(res);
        }

        [HttpGet("FetchSubscriptionPlan")]
        public async Task<ActionResult<SubscriptionPlanDto>> GetAllSubscriptionPlans()
        {
            //var result = new List<SubscriptionPlanDto>();
            var result = await _subscriptionPlanService.FetchSubscriptionPlan();
            return Ok(result);
            //for (int i = 0; i < savedInDb.Count; i++)
            //{
            //    var entityFromStripe = _stripeProductsService.GetStripeProduct(savedInDb[i].Id);
            //    await _subscriptionPlanService.UpsertSubscriptionPlan(entityFromStripe);
            //    result.Add(entityFromStripe);
            //}
            //return result;
        }

        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteSubscriptionPlan(string stripeProductId)
        {
            await _stripeProductsService.DeleteStripeProduct(stripeProductId);
            await _subscriptionPlanService.DeleteSubscriptionPlan(stripeProductId);
            return NoContent();
        }
    }
}
