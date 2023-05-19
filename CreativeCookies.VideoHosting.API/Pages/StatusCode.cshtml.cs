using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CreativeCookies.VideoHosting.API.Pages
{
    public class StatusCodeModel : PageModel
    {
        private readonly IConfiguration _configuration;
        [BindProperty(SupportsGet = true)]
        public string StatusCode { get; set; }

        public string ClientUrl { get; set; }
        public string WebsiteName { get; set; }

        public StatusCodeModel(IConfiguration configuration)
        {
            _configuration = configuration;
            ClientUrl = _configuration.GetValue<string>(nameof(ClientUrl));
            WebsiteName = _configuration.GetValue<string>(nameof(WebsiteName));

        }

        public void OnGet()
        {
        }
    }
}
