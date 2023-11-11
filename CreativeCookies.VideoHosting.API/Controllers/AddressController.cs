using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.Repositories.OAuth;
using CreativeCookies.VideoHosting.Contracts.Services;
using CreativeCookies.VideoHosting.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CreativeCookies.VideoHosting.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;
        private readonly IAccessTokenService _accessTokenService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AddressController(IAddressService addressService, IAccessTokenService accessTokenService, IHttpContextAccessor httpContextAccessor)
        {
            _addressService = addressService;
            _accessTokenService = accessTokenService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public async Task<ActionResult<InvoiceAddressDto>> GetByUserId()
        {
            var cookie = _httpContextAccessor.HttpContext.Request.Cookies["stac"];
            if (string.IsNullOrEmpty(cookie))
            {
                return new StatusCodeResult(401);
            }
            var userId = _accessTokenService.GetUserIdFromToken(cookie);
            var address = await _addressService.GetAddress(userId);
            return Ok(address);
        }

        [HttpPut]
        public async Task<ActionResult<InvoiceAddressDto>> UpsertAddress([FromBody] InvoiceAddressDto newAddress)
        {
            var cookie = _httpContextAccessor.HttpContext.Request.Cookies["stac"];
            if (string.IsNullOrEmpty(cookie))
            {
                return new StatusCodeResult(401);
            }

            var userId = _accessTokenService.GetUserIdFromToken(cookie);
            newAddress.UserId = userId.ToLowerInvariant();
            var res = await _addressService.UpsertAddress(newAddress);
            return Ok(res);
        }
    }
}
