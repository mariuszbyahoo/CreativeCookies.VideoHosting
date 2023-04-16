using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CreativeCookies.VideoHosting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        public ActionResult<string> GetIndex()
        {
            return Ok("And my new Release pipeline works!!! - This was non existent prior CI&CD");
        }
    }
}
