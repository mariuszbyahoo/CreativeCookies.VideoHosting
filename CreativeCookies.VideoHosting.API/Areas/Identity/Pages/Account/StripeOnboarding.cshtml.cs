using CreativeCookies.VideoHosting.Contracts.Enums;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CreativeCookies.VideoHosting.API.Areas.Identity.Pages.Account
{
    public class StripeOnboardingModel : PageModel
    {
        private readonly IStripeService _stripeService;
        public StripeConnectAccountStatus AccountStatus { get; set; }

        public StripeOnboardingModel(IStripeService stripeService)
        {
            _stripeService = stripeService;
        }

        public void OnGet()
        {
            var accountId = _stripeService.GetConnectedAccountsId().Result;
            if (!string.IsNullOrEmpty(accountId))
            {
                AccountStatus = _stripeService.ReturnAccountStatus(accountId);
            }
            else
            {
                AccountStatus = StripeConnectAccountStatus.Disconnected; 
            }
        }

        public async Task<IActionResult> OnPostConnect()
        {
            var url = await _stripeService.ReturnConnectAccountLink();
            return Redirect(url);
        }

        public async Task<IActionResult> OnPostDelete()
        {
            await _stripeService.DeleteConnectAccounts();
            return RedirectToPage(); // Return to the current page or redirect elsewhere if needed
        }
    }

}
