using CreativeCookies.VideoHosting.Contracts.Enums;
using CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.DTOs.Stripe;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace CreativeCookies.VideoHosting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StripeController : ControllerBase
    {
        private readonly IConnectAccountsService _connectAccountsSrv;
        private readonly IStripeOnboardingService _stripeService;
        private readonly ILogger<StripeController> _logger;
        private readonly IConfiguration _configuration;

        public StripeController(
            IConnectAccountsService connectAccountsSrv, IStripeOnboardingService stripeService, 
            ILogger<StripeController> logger, IConfiguration configuration)
        {
            _connectAccountsSrv = connectAccountsSrv;
            _stripeService = stripeService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet("IsSetUp")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin,ADMIN")]
        public async Task<ActionResult<StripeResultDto<StripeConnectAccountStatus>>> IsStripeAccountSetUp()
        {
            StripeResultDto<StripeConnectAccountStatus> result = new StripeResultDto<StripeConnectAccountStatus>(true, StripeConnectAccountStatus.Disconnected, "account_missing");
            var idStoredInDatabase = await _connectAccountsSrv.GetConnectedAccountId();
            if (!string.IsNullOrWhiteSpace(idStoredInDatabase))
            {
                var olderThanMinute = await _connectAccountsSrv.CanBeQueriedOnStripe(idStoredInDatabase);
                if (olderThanMinute) result = _stripeService.GetAccountStatus(idStoredInDatabase);
                else 
                { 
                    result.Data = StripeConnectAccountStatus.PendingSave;
                    result.Success = true;
                    result.ErrorMessage = "An account has been recently submitted, check at least one minute after creating";
                }
            }
            return Ok(result);
        }

        [HttpGet("OnboardingRefresh")]
        public async Task<IActionResult> RefreshOnboarding()
        {
            var accountLinkResponse = _stripeService.GenerateConnectAccountLink();
            return Redirect(accountLinkResponse.Data.AccountOnboardingUrl);
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
                    await _connectAccountsSrv.EnsureSaved(account.Id);
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
    }
}
