using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CreativeCookies.VideoHosting.API.Pages
{
    public class StatusCodeModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<StatusCodeModel> _logger;

        [BindProperty(SupportsGet = true)]
        public string StatusCode { get; set; }

        public string ClientUrl { get; set; }
        public string WebsiteName { get; set; }

        public StatusCodeModel(IConfiguration configuration, ILogger<StatusCodeModel> logger)
        {
            _configuration = configuration;
            _logger = logger;
            ClientUrl = _configuration.GetValue<string>(nameof(ClientUrl));
            WebsiteName = _configuration.GetValue<string>(nameof(WebsiteName));
        }

        public void OnGet()
        {
            int statusCode;
            if(int.TryParse(StatusCode, out statusCode) && statusCode > 499)
            {
                _logger.LogError($"An unexpected error occured, response sent with status code: {statusCode} user redirected to /StatusCode page");
            }
        }
    }
}
