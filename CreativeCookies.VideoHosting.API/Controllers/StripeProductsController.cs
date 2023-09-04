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

        [HttpGet("FetchSubscriptionPlan")]
        public async Task<ActionResult<SubscriptionPlanDto>> GetAllSubscriptionPlans()
        {
            var result = new List<SubscriptionPlanDto>();
            return await _subscriptionPlanService.FetchSubscriptionPlan();
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

        [HttpPost("ProductWebhook")]
        public async Task<IActionResult> ProductWebHook()
        {
            string endpointSecret = _configuration.GetValue<string>("WebhookEndpointSecret");

            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"],
                    endpointSecret);

                if (stripeEvent.Type == Events.ProductCreated)
                {
                    // HACK TODO IMPLEMENT LOGIC
                }
                else if (stripeEvent.Type == Events.ProductUpdated)
                {
                    // HACK TODO IMPLEMENT LOGIC
                }
                else if (stripeEvent.Type == Events.ProductDeleted)
                {
                    // HACK TODO IMPLEMENT LOGIC
                }
                else
                {
                    _logger.LogWarning($"Unexpected Stripe event's type: {stripeEvent.ToJson()}");
                    return BadRequest();
                }
            }
            catch (StripeException e)
            {
                _logger.LogError(e, e.Message);
                return BadRequest("Stripe exception occured");
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message, e.StackTrace);
            }

            return Ok();
        }
    }
}
