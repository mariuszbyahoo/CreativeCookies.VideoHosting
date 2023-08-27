using CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe;
using CreativeCookies.VideoHosting.DTOs.Stripe;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace CreativeCookies.VideoHosting.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ADMIN,admin")]
    [ApiController]
    public class StripeProductsController : ControllerBase
    {
        private readonly IStripeProductsService _stripeProductsService;

        public StripeProductsController(IStripeProductsService stripeProductsService)
        {
            _stripeProductsService = stripeProductsService;
        }

        public async Task<ActionResult<SubscriptionPlanDto>> CreateSubscriptionPlan(string name, string description)
        {
            if (string.IsNullOrEmpty(name)) return BadRequest("Name cannot be empty string");
            if (string.IsNullOrWhiteSpace(description)) return BadRequest("Description cannot be an empty string");
            var res = _stripeProductsService.CreateStripeProduct(name, description);
            return Ok(res);
        }

        public async Task<ActionResult<PriceDto>> CreateStripePrice(string stripeProductId, string currencyCode, int unitAmount)
        {
            if (string.IsNullOrWhiteSpace(stripeProductId)) return BadRequest($"StripeProductId cannot be empty");
            if (unitAmount <= 0) return BadRequest($"Amount has to be greater than 0");
            if (string.IsNullOrWhiteSpace(currencyCode) || currencyCode.Length != 3) return BadRequest($"Currency Code has to be three letter long : https://stripe.com/docs/currencies");
            var res = _stripeProductsService.CreateStripePrice(stripeProductId, currencyCode, unitAmount);
            return Ok(res);
        }
    }
}
