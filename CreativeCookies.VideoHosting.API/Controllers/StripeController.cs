using CreativeCookies.VideoHosting.Contracts.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CreativeCookies.VideoHosting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StripeController : ControllerBase
    {
        private readonly IStripeService _stripeService;

        public StripeController(IStripeService stripeService)
        {
            _stripeService = stripeService;
        }

        [HttpGet("IsSetUp")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin,ADMIN")]
        public async Task<ActionResult<bool>> IsStripeAccountSetUp()
        {
            // HACK: TODO, also verify is this Stripe Connected Account's Id present in the set of connected accounts of myhub.com.pl connected accounts.
            bool result = false;
            var idStoredInDatabase = await _stripeService.GetConnectedAccountsId();
            if (!string.IsNullOrWhiteSpace(idStoredInDatabase))
            {
                result = _stripeService.IsDbRecordValid(idStoredInDatabase);
            }
            return Ok(result);
        }
    }
}
