using CreativeCookies.VideoHosting.Contracts.Services.About;
using CreativeCookies.VideoHosting.DTOs.About;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CreativeCookies.VideoHosting.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AboutController : ControllerBase
    {
        private readonly IAboutPageService _aboutPageService;

        public AboutController(IAboutPageService aboutPageService)
        {
            _aboutPageService = aboutPageService;
        }

        [HttpGet]
        public async Task<ActionResult<string>> GetAboutPageContent()
        {
            var res = await _aboutPageService.GetAboutPageContents();
            return Ok(res);
        }

        [HttpPut]
        [Authorize(Roles = "admin,ADMIN,Admin")]
        public async Task<ActionResult<bool>> UpsertAboutPageContent([FromBody] AboutPageDTO newContent)
        {
            if (newContent == null) return BadRequest();
            var res = await _aboutPageService.UpsertAboutPageContents(newContent.InnerHTML);
            return Ok(res);
        }
    }
}
