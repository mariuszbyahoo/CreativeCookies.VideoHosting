using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.DTOs.Regulations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CreativeCookies.VideoHosting.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RegulationsController : ControllerBase
    {
        private readonly IRegulationsRepository _repo;

        public RegulationsController(IRegulationsRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("regulations")]
        public async Task<ActionResult<WebsiteRegulationsDTO>> GetRegulations()
        {
            var dto = await _repo.GetRegulations();
            return Ok(dto);
        }

        [HttpPut("regulations")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin,ADMIN")]
        public async Task<ActionResult<WebsiteRegulationsDTO>> UpdateRegulations([FromBody] WebsiteRegulationsDTO dto)
        {
            var updateResult = await _repo.UpdateRegulations(dto);

            return Ok(updateResult);
        }

        [HttpGet("privacyPolicy")]
        public async Task<ActionResult<WebsitePrivacyPolicyDTO>> GetPrivacyPolicy()
        {
            var dto = await _repo.GetPrivacyPolicy();
            return Ok(dto);
        }

        [HttpPut("privacyPolicy")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin,ADMIN")]
        public async Task<ActionResult<WebsitePrivacyPolicyDTO>> UpdatePrivacyPolicy([FromBody] WebsitePrivacyPolicyDTO dto)
        {
            var updateResult = await _repo.UpdatePrivacyPolicy(dto);

            return Ok(updateResult);
        }
    }
}
