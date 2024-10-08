using CreativeCookies.VideoHosting.Contracts.Enums;
using CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.DTOs.Stripe;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CreativeCookies.VideoHosting.API.Areas.Identity.Pages.Account
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin,ADMIN")]
    public class StripeOnboardingModel : PageModel
    {
        private readonly IConnectAccountsService _connectAccountsSrv;
        private readonly IStripeOnboardingService _stripeService;
        private readonly ILogger<StripeOnboardingModel> _logger;
        public StripeConnectAccountStatus AccountStatus { get; set; }

        public StripeOnboardingModel(IConnectAccountsService connectAccountsSrv, IStripeOnboardingService stripeService, ILogger<StripeOnboardingModel> logger)
        {
            _connectAccountsSrv = connectAccountsSrv;
            _stripeService = stripeService;
            _logger = logger;
        }

        public async Task OnGet()
        {
            AccountStatus = StripeConnectAccountStatus.Disconnected;
            var connectedAccountId = _connectAccountsSrv.GetConnectedAccountId();
            TempData["ConnectedAccountId"] = connectedAccountId;
            var isEligibleToQueryAPI = await _connectAccountsSrv.CanBeQueriedOnStripe(connectedAccountId);
            if (!string.IsNullOrEmpty(connectedAccountId) && isEligibleToQueryAPI)
            {
                var result = _stripeService.GetAccountStatus(connectedAccountId);
                if (result.Success)
                {
                    AccountStatus = result.Data;
                } 
                else
                {
                    _logger.LogError($"Unexpected error occured inside StripeOnboardingViewModel: {result.ErrorMessage}");
                    LocalRedirect($"~/StatusCode?statusCode={ExceptionCodes.StripeIntegrationException}");
                }
            }
            else if (!string.IsNullOrEmpty(connectedAccountId) && !isEligibleToQueryAPI)
            {
                AccountStatus = StripeConnectAccountStatus.PendingSave;
            }
            TempData["AccountStatus"] = AccountStatus;
        }

        public async Task<IActionResult> OnPostConnect()
        {
            StripeResultDto<AccountCreationResultDto> accountLinkResult = null;
            var accountStatus = (StripeConnectAccountStatus)Enum.Parse(typeof(StripeConnectAccountStatus), TempData["AccountStatus"]?.ToString());
            var connectedAccountId = TempData["ConnectedAccountId"]?.ToString() ?? string.Empty;

            if (accountStatus == StripeConnectAccountStatus.Disconnected)
            {
                accountLinkResult = _stripeService.GenerateConnectAccountLink();
            }
            else if (accountStatus == StripeConnectAccountStatus.Restricted)
            {
                if (string.IsNullOrWhiteSpace(connectedAccountId))
                    accountLinkResult = _stripeService.GenerateConnectAccountLink();
                else
                    accountLinkResult = _stripeService.GenerateConnectAccountLink(connectedAccountId);
            }
            else
            {
                // Stripe correctly connected or is not eligible to be queried, this actually shouldn't being fired at all.
                return Page();
            }

            if (accountLinkResult != null && accountLinkResult.Success)
            {
                await _connectAccountsSrv.EnsureSaved(accountLinkResult.Data.AccountId);
                return Redirect(accountLinkResult.Data.AccountOnboardingUrl);
            }
            else 
            {
                _logger.LogError($"Unexpected error occured inside StripeOnboardingViewModel, accountLinkResult's errorMsg: {accountLinkResult?.ErrorMessage}");
                return LocalRedirect($"~/StatusCode?statusCode={ExceptionCodes.StripeIntegrationException}");
            }
        }
    }
}
