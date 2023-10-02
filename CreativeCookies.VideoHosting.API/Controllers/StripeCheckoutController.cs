using CreativeCookies.VideoHosting.API.DTOs;
using CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe;
using CreativeCookies.VideoHosting.Contracts.Services.IdP;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CreativeCookies.VideoHosting.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class StripeCheckoutController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripeProductsController> _logger;
        private readonly ICheckoutService _checkoutService;
        private readonly JwtSecurityTokenHandler _tokenHandler;
        private readonly IMyHubUserManager _userManager;

        public StripeCheckoutController(IConfiguration configuration, ILogger<StripeProductsController> logger, ICheckoutService checkoutService, IMyHubUserManager userManager)
        {
            _configuration = configuration;
            _logger = logger;
            _checkoutService = checkoutService;
            _userManager = userManager;
            _tokenHandler = new JwtSecurityTokenHandler();
        }

        [HttpPost("CreateSession")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] StripeCreateSessionRequestDto dto)
        {
            try
            {
                var accessToken = Request.Cookies["stac"];
                if (_tokenHandler.CanReadToken(accessToken))
                {
                    var token = _tokenHandler.ReadJwtToken(accessToken);
                    var emailClaim = token.Claims.FirstOrDefault(c => c.Type.Equals("email", StringComparison.InvariantCultureIgnoreCase));
                    var user = await _userManager.FindByEmailAsync(emailClaim.Value);
                    if (string.IsNullOrWhiteSpace(dto.PriceId)) return BadRequest("PriceId is required");
                    var sessionUrl = await _checkoutService.CreateNewSession(dto.PriceId, user.StripeCustomerId); // HACK Task 178 : Pass dto.IsCoolingOffPeriodApplicable into CreateNewSession

                    return Ok(new StripeCreateSessionResponseDto(sessionUrl));
                }
                return BadRequest("Invalid access token used, log in again");
            }
            catch (StripeException ex)
            {
                _logger.LogError($"Stripe error: {ex.Message}");
                return new ObjectResult(ex.Message) { StatusCode = StatusCodes.Status400BadRequest };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error: {ex}");
                return new ObjectResult(ex.Message) { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        [HttpGet("Status")]
        public async Task<ActionResult<bool>> CheckSessionStatus([FromQuery] string sessionId)
        {
            try
            {
                var res = await _checkoutService.IsSessionPaymentPaid(sessionId);
                return Ok(res);
            }
            catch(StripeException ex)
            {
                _logger.LogError($"Stripe error: {ex.Message}");
                return new ObjectResult(ex.Message) { StatusCode = StatusCodes.Status400BadRequest };
            }
            catch(Exception ex)
            {
                _logger.LogError($"Unexpected error: {ex}");
                return new ObjectResult(ex.Message) { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }
    }
}
