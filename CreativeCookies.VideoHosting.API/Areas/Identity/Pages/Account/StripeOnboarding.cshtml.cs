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

        public void OnGet()
        {
            var accountId = _connectAccountsRepo.GetConnectedAccountId().Result;
            if (!string.IsNullOrEmpty(accountId))
            {
                AccountStatus = _connectAccountsRepo.ReturnAccountStatus(accountId);
            }
            else
            {
                AccountStatus = StripeConnectAccountStatus.Disconnected; 
            }
        }

        public async Task<IActionResult> OnPostConnect()
        {
            var url = await _connectAccountsRepo.ReturnConnectAccountLink();
            return Redirect(url);
        }
    }

}
