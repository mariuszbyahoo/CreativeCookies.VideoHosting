using CreativeCookies.VideoHosting.API.DTOs;
using CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace CreativeCookies.VideoHosting.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StripeCheckoutController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripeProductsController> _logger;
        private readonly ICheckoutService _checkoutService;
        

        public StripeCheckoutController(IConfiguration configuration, ILogger<StripeProductsController> logger, ICheckoutService checkoutService)
        {
            _configuration = configuration;
            _logger = logger;
            _checkoutService = checkoutService;
        }

        [HttpPost("CreateSession")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] StripeCreateSessionRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PriceId)) return BadRequest("PriceId is required");
            var sessionUrl = await _checkoutService.CreateNewSession(dto.PriceId);

            return Ok(new StripeCreateSessionResponseDto(sessionUrl));
        }
    }
}
