using CreativeCookies.VideoHosting.Contracts.Models;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<IVideo>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<IVideo>>> GetAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await _repository.GetAll(cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IVideo))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IVideo>> GetSingleAsync(Guid id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await _repository.GetVideo(id, cancellationToken);

            if(result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(IVideo))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IVideo>> PostAsync([FromBody] IVideo video, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (video == null) return BadRequest();

            var result = await _repository.PostVideo(video, cancellationToken);

            return CreatedAtAction(nameof(PostAsync), result);
        }

        [HttpPatch]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IVideo))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IVideo>> PatchAsync([FromBody] IVideo video, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if(video == null) return BadRequest();

            if (await _repository.IsPresentInDatabase(video.Id, cancellationToken))
            {

                var result = await _repository.UpdateVideo(video, cancellationToken);

                return Ok(result);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task <IActionResult> DeleteAsync([FromQuery] Guid id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if(await _repository.IsPresentInDatabase(id, cancellationToken))
            {
                await _repository.DeleteVideo(id, cancellationToken);

                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }
    }
}
