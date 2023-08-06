using CreativeCookies.VideoHosting.Contracts.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CreativeCookies.VideoHosting.API.Areas.Identity.Pages.Account
{
    public class StripeOnboardingModel : PageModel
    {
        private readonly IStripeService _stripeService;

        public StripeOnboardingModel(IStripeService stripeService)
        {
            _stripeService = stripeService;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            var url = _stripeService.ReturnConnectAccountLink();
            return Redirect(url);
        }
    }
}
