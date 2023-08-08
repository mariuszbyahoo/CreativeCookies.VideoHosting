using CreativeCookies.VideoHosting.Contracts.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace CreativeCookies.VideoHosting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StripeController : ControllerBase
    {
        private readonly IStripeService _stripeService;
        private readonly ILogger<StripeController> _logger;
        private readonly IConfiguration _configuration;

        public StripeController(IStripeService stripeService, ILogger<StripeController> logger, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _stripeService = stripeService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet("IsSetUp")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin,ADMIN")]
        public async Task<ActionResult<bool>> IsStripeAccountSetUp()
        {
            bool result = false;
            var idStoredInDatabase = await _stripeService.GetConnectedAccountsId();
            if (!string.IsNullOrWhiteSpace(idStoredInDatabase))
            {
                result = _stripeService.IsDbRecordValid(idStoredInDatabase);
            }
            return Ok(result);
        }

        [HttpGet("OnboardingRefresh")]
        public IActionResult RefreshOnboarding()
        {
            var accountLink = _stripeService.ReturnConnectAccountLink();
            return Redirect(accountLink);
        }
        [HttpGet("OnboardingReturn")]
        public async Task<IActionResult> OnboardingReturn()
        {
            return Ok("Onboarding Returned!");
            // What to do here? Is there anything to be done specifically? Because even before hitting this
            // controller, the account's ID is being saved to the database.
        }

        [HttpPost("WebHook")]
        public async Task<IActionResult> AccountUpdatedWebHook()
        {
            string endpointSecret = _configuration.GetValue<string>("WebhookEndpointSecret");

            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"],
                    endpointSecret);

                if (stripeEvent.Type == Events.AccountUpdated)
                {
                    var account = stripeEvent.Data.Object as Account;
                    await _stripeService.SaveAccountId(account.Id);
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
                return BadRequest();
            }

            return Ok();
        }

    }
}
