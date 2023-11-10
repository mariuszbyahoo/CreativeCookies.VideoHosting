using CreativeCookies.VideoHosting.Contracts.Services;
using CreativeCookies.VideoHosting.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CreativeCookies.VideoHosting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,admin,ADMIN")]
    public class MerchantController : ControllerBase
    {
        private readonly IMerchantService _merchantService;

        public MerchantController(IMerchantService merchantService)
        {
            _merchantService = merchantService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMerchant()
        {
            var res = await _merchantService.GetMerchant();

            return Ok(res);
        }

        [HttpPut]
        public async Task<IActionResult> UpsertMerchant([FromBody] MerchantDto newMerchant)
        {
            var res = await _merchantService.UpsertMerchant(newMerchant);
            return Ok(res);
        }
    }
}
