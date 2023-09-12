using CreativeCookies.VideoHosting.API.DTOs;
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

        #region Products

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

        [HttpDelete("DeleteSubscriptionPlan")]
        public async Task<IActionResult> DeleteSubscriptionPlan(string stripeProductId)
        {
            await _stripeProductsService.DeleteStripeProduct(stripeProductId);
            await _subscriptionPlanService.DeleteSubscriptionPlan(stripeProductId);
            return NoContent();
        }

        [HttpGet("FetchSubscriptionPlan")]
        public async Task<ActionResult<SubscriptionPlanDto>> GetAllSubscriptionPlan()
        {
            var result = await _subscriptionPlanService.FetchSubscriptionPlan();
            return Ok(result);
        }

        #endregion

        #region prices

        [HttpPost("CreateStripePrice")]
        public async Task<ActionResult<PriceDto>> CreateStripePrice([FromBody] StripePriceCreationDto model)
        {
            if (string.IsNullOrWhiteSpace(model.StripeProductId)) return BadRequest($"StripeProductId cannot be empty");
            if (model.UnitAmount <= 0) return BadRequest($"Amount has to be greater than 0");
            if (string.IsNullOrWhiteSpace(model.CurrencyCode) || model.CurrencyCode.Length != 3) return BadRequest($"Currency Code has to be three letter long : https://stripe.com/docs/currencies");
            var res = await _stripeProductsService.CreateStripePrice(model.StripeProductId, model.CurrencyCode, model.UnitAmount);
            return Ok(res);
        }

        [HttpGet("GetAllPrices")]
        public async Task<ActionResult<IEnumerable<PriceDto>>> GetAll([FromQuery] string productId)
        {
            if (string.IsNullOrWhiteSpace(productId)) return BadRequest("productId is required");
            var res = await _stripeProductsService.GetStripePrices(productId);
            return Ok(res);
        }

        [HttpGet("GetPriceById")]
        public ActionResult<IEnumerable<PriceDto>> GetPriceById([FromQuery] string priceId)
        {
            if (string.IsNullOrWhiteSpace(priceId)) return BadRequest("priceId is required");
            var res = _stripeProductsService.GetStripePrices(priceId);
            return Ok(res);
        }


        [HttpPut("TogglePriceState")]
        public async Task<ActionResult<PriceDto>> DeactivateStripePrice([FromQuery] string priceId)
        {
            if (string.IsNullOrWhiteSpace(priceId)) return BadRequest("PriceId cannot be empty");
            var res = await _stripeProductsService.TogglePriceState(priceId);
            return Ok(res);
        }

        #endregion
    }
}
