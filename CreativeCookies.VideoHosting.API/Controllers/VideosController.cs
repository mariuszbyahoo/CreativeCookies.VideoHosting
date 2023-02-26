using CreativeCookies.VideoHosting.Contracts.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CreativeCookies.VideoHosting.API.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class VideosController : ControllerBase
    {
        private readonly IVideosRepository _repository;

        public VideosController(IVideosRepository repository)
        {
            _repository = repository;
        }

        [HttpGet] 
        public IActionResult Get()
        {
            return Ok("Yep everything running as it should...");
        }
    }
}
