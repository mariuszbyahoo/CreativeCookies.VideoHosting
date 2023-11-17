using CreativeCookies.VideoHosting.API.DTOs;
using CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe;
using CreativeCookies.VideoHosting.Contracts.Services;
using CreativeCookies.VideoHosting.Contracts.Services.IdP;
using CreativeCookies.VideoHosting.DTOs;
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
        private readonly IAddressService _addressService;

        public StripeCheckoutController(IConfiguration configuration, ILogger<StripeProductsController> logger, 
            ICheckoutService checkoutService, IMyHubUserManager userManager, IUsersService usersService, IAddressService addressService)
        {
            _configuration = configuration;
            _logger = logger;
            _usersService = usersService;
            _checkoutService = checkoutService;
            _userManager = userManager;
            _tokenHandler = new JwtSecurityTokenHandler();
            _addressService = addressService;
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
                        dto.Address.Country = "Polska";
                        var invoiceAddress = new InvoiceAddressDto(dto.Address.Id, dto.Address.FirstName, dto.Address.LastName,
                            dto.Address.Street, dto.Address.HouseNo, dto.Address.AppartmentNo,
                            dto.Address.PostCode, dto.Address.City, dto.Address.Country, user.Id.ToString());
                        var res = await _addressService.UpsertAddress(invoiceAddress);

                        if (res != 1)
                        {
                            return StatusCode(400, $"New address for user: {user.UserEmail} has not been added, result of upsert operation was different than 1");
                        }

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
