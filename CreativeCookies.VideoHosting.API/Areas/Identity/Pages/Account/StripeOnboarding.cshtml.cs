using CreativeCookies.VideoHosting.Contracts.Enums;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.Stripe;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CreativeCookies.VideoHosting.API.Areas.Identity.Pages.Account
{
    public class StripeOnboardingModel : PageModel
    {
        private readonly IConnectAccountsRepository _connectAccountsRepo;
        private readonly IStripeService _stripeService;
        public StripeConnectAccountStatus AccountStatus { get; set; }

        public StripeOnboardingModel(IConnectAccountsRepository connectAccountsRepo, IStripeService stripeService)
        {
            _connectAccountsRepo = connectAccountsRepo;
            _stripeService = stripeService;
        }

        public async Task OnGet()
        {
            var accountId = await _connectAccountsRepo.GetConnectedAccountId();
            if (!string.IsNullOrEmpty(accountId))
            {
                var result = _stripeService.GetAccountStatus(accountId);
                if (result.Success)
                    AccountStatus = result.Data;
                else
                    AccountStatus = StripeConnectAccountStatus.Disconnected;
            }
            else
            {
                AccountStatus = StripeConnectAccountStatus.Disconnected;
            }
        }

        public IActionResult OnPostConnect()
        {
            var accountLinkResult = _stripeService.GenerateConnectAccountLink();
            if (accountLinkResult.Success)
                return Redirect(accountLinkResult.Data.AccountOnboardingUrl);
            else
                return BadRequest("Exception occured inside of a StripeService, check Logs.");
        }
    }

}
