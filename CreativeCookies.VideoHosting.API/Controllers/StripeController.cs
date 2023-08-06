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

        public StripeController(IStripeService stripeService, ILogger<StripeController> logger)
        {
            _stripeService = stripeService;
            _logger = logger;
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
        [HttpGet("OnboardingReturnUrl")]
        public async Task<IActionResult> OnboardingReturn()
        {
            return Ok("Onboarding Returned!");
        }

        [HttpPost("WebHook")]
        public async Task<IActionResult> AccountUpdatedWebHook()
        {
            const string endpointSecret = "whsec_5a47597a9ce53e2107dba3f79794a4853847ed41c8281625895196654c06271a";

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
