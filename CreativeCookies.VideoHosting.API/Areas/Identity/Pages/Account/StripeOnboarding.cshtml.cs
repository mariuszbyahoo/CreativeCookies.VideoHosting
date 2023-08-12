using CreativeCookies.VideoHosting.Contracts.Enums;
using CreativeCookies.VideoHosting.Contracts.ExceptionCodes;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.Stripe;
using CreativeCookies.VideoHosting.Contracts.Wrappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CreativeCookies.VideoHosting.API.Areas.Identity.Pages.Account
{
    public class StripeOnboardingModel : PageModel
    {
        private readonly IConnectAccountsRepository _connectAccountsRepo;
        private readonly IStripeService _stripeService;
        private readonly ILogger<StripeOnboardingModel> _logger;
        public StripeConnectAccountStatus AccountStatus { get; set; }

        public StripeOnboardingModel(IConnectAccountsRepository connectAccountsRepo, IStripeService stripeService, ILogger<StripeOnboardingModel> logger)
        {
            _connectAccountsRepo = connectAccountsRepo;
            _stripeService = stripeService;
            _logger = logger;
        }

        public async Task OnGet()
        {
            var accountStatus = StripeConnectAccountStatus.Disconnected;
            var connectedAccountId = await _connectAccountsRepo.GetConnectedAccountId();
            TempData["ConnectedAccountId"] = connectedAccountId;
            if (!string.IsNullOrEmpty(connectedAccountId))
            {
                var result = _stripeService.GetAccountStatus(connectedAccountId);
                if (result.Success)
                {
                    accountStatus = result.Data;
                }
                else
                {
                    _logger.LogError($"Unexpected error occured inside StripeOnboardingViewModel: {result.ErrorMessage}");
                    LocalRedirect($"~/StatusCode?statusCode={ExceptionCodes.StripeIntegrationException}");
                }
            }
            TempData["AccountStatus"] = accountStatus;
        }

        public async Task<IActionResult> OnPostConnect()
        {
            IStripeResult<IAccountCreationResult> accountLinkResult = null;
            var accountStatus = (StripeConnectAccountStatus)Enum.Parse(typeof(StripeConnectAccountStatus), TempData["AccountStatus"]?.ToString());
            var connectedAccountId = TempData["ConnectedAccountId"]?.ToString() ?? string.Empty;
            if (accountStatus == StripeConnectAccountStatus.Disconnected)
            {
                if (!string.IsNullOrWhiteSpace(connectedAccountId))
                {
                    // HACK TODO: Delete an account from stripe
                    await _connectAccountsRepo.DeleteConnectAccounts();
                }
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
                // Stripe correctly connected, this actually shouldn't being fired at all.
                return Page();
            }
            if (accountLinkResult != null && accountLinkResult.Success)
            {
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
