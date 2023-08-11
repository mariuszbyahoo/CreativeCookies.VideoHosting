using CreativeCookies.VideoHosting.Contracts.Enums;
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
        private readonly IConnectAccountsRepository _stripeService;
        private readonly ILogger<StripeController> _logger;
        private readonly IConfiguration _configuration;

        public StripeController(IConnectAccountsRepository stripeService, ILogger<StripeController> logger, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _stripeService = stripeService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet("IsSetUp")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin,ADMIN")]
        public async Task<ActionResult<StripeConnectAccountStatus>> IsStripeAccountSetUp()
        {
            var result = StripeConnectAccountStatus.Disconnected;
            var idStoredInDatabase = await _stripeService.GetConnectedAccountsId();
            if (!string.IsNullOrWhiteSpace(idStoredInDatabase))
            {
                result = _stripeService.ReturnAccountStatus(idStoredInDatabase);
            }
            return Ok(result);
        }

        [HttpGet("OnboardingRefresh")]
        public async Task<IActionResult> RefreshOnboarding()
        {
            var accountLink = await _stripeService.ReturnConnectAccountLink();
            return Redirect(accountLink);
        }

        [HttpPost("AccountUpdatedWebhook")]
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
                    if (_stripeService.ReturnAccountStatus(await _stripeService.GetConnectedAccountsId()) == StripeConnectAccountStatus.Disconnected)
                    {
                        await _stripeService.SaveAccountId(account.Id);
                    }
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
            catch(Exception e)
            {
                _logger.LogError(e, e.Message, e.StackTrace);
            }

            return Ok();
        }

        [HttpDelete("DeleteStoredAccounts")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin,ADMIN")]
        public async Task<IActionResult> DeleteStoredAccounts()
        {
            await _stripeService.DeleteConnectAccounts();
            return NoContent();
        }
    }
}
