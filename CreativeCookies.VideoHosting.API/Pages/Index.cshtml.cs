using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CreativeCookies.VideoHosting.API.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public string ClientUrl { get; set; }
        public string WebsiteName { get; set; }

        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
            ClientUrl = _configuration.GetValue<string>(nameof(ClientUrl));
            WebsiteName = _configuration.GetValue<string>(nameof(WebsiteName));
        }
    }
}
