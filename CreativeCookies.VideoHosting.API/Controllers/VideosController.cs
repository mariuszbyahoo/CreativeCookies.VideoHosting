using CreativeCookies.VideoHosting.Contracts.Models;
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
        private readonly IHostApplicationLifetime _appLifetime;

        public VideosController(IVideosRepository repository, IHostApplicationLifetime appLifetime)
        {
            _repository = repository;
            _appLifetime = appLifetime;
        }

        [HttpGet] 
        public async Task<ActionResult<IEnumerable<IVideo>>> Get(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await _repository.GetAll(cancellationToken);

            return Ok(result);
        }
    }
}
