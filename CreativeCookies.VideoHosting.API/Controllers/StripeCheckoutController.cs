using CreativeCookies.VideoHosting.API.DTOs;
using CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe;
using CreativeCookies.VideoHosting.Contracts.Services;
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
        private readonly IUsersService _usersService;

        public StripeCheckoutController(IConfiguration configuration, ILogger<StripeProductsController> logger, 
            ICheckoutService checkoutService, IMyHubUserManager userManager, IUsersService usersService)
        {
            _configuration = configuration;
            _logger = logger;
            _usersService = usersService;
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

                    if (user?.StripeCustomerId == null) throw new InvalidDataException($"User with email {user?.UserEmail} has no StripeCustomerID set!");

                    var datesActive = user.SubscriptionStartDateUTC < DateTime.UtcNow && user.SubscriptionEndDateUTC > DateTime.UtcNow;
                    var userHasSubscription = await _checkoutService.HasUserActiveSubscription(user.StripeCustomerId);
                    if (string.IsNullOrWhiteSpace(dto.PriceId)) return BadRequest("PriceId is required");
                    var isUserWithinCoolingOffPeriod = _usersService.HasUserAScheduledSubscription(user.HangfireJobId);
                    _logger.LogInformation($"User: {user.StripeCustomerId} has requested to create a new Checkout session where datesActive: {datesActive}, userHasSubscription: {userHasSubscription}, and isUserWithinCoolingOffPeriod: {isUserWithinCoolingOffPeriod}");

                    // User has an ongoing subscription
                    if (datesActive && userHasSubscription && !isUserWithinCoolingOffPeriod)
                    {
                        return StatusCode(409, "User already has an active subscription");
                    }
                    // User has a subscription scheduled for the future
                    else if (!datesActive && !userHasSubscription && isUserWithinCoolingOffPeriod)
                    {
                        return StatusCode(423, "User has a future scheduled subscription");
                    }
                    // User has no subscription, either ongoing or scheduled
                    else if (!datesActive && !userHasSubscription && !isUserWithinCoolingOffPeriod)
                    {
                        var sessionUrl = await _checkoutService.CreateNewSession(dto.PriceId, user.StripeCustomerId, dto.HasDeclinedCoolingOffPeriod);
                        return Ok(new StripeCreateSessionResponseDto(sessionUrl));
                    }
                    else
                    {
                        return StatusCode(400, "Some edge case occured, please investigate");
                    }
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
