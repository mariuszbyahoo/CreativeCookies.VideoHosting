using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CreativeCookies.VideoHosting.API.Areas.Identity.Pages.Account
{
    public class ConfirmAccountModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public string WebsiteName { get; set; }
        public ConfirmAccountModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void OnGet()
        {
            WebsiteName = _configuration.GetValue<string>("WebsiteName");
        }
    }
}
