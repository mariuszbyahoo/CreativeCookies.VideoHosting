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
    [Route("[controller]")]
    [ApiController]
    public class StripeAccountsController : ControllerBase
    {
        private readonly IConnectAccountsService _connectAccountsSrv;
        private readonly IStripeOnboardingService _stripeService;
        private readonly ILogger<StripeAccountsController> _logger;
        private readonly IConfiguration _configuration;

        public StripeAccountsController(
            IConnectAccountsService connectAccountsSrv, IStripeOnboardingService stripeService, 
            ILogger<StripeAccountsController> logger, IConfiguration configuration)
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
            var idStoredInDatabase = _connectAccountsSrv.GetConnectedAccountId();
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
    }
}
