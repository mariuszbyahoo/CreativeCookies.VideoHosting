using CreativeCookies.VideoHosting.API.Models;
using CreativeCookies.VideoHosting.Contracts.Models;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Net.Mime;
using System.Text;

namespace CreativeCookies.VideoHosting.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class VideosController : ControllerBase
    {
        private readonly IVideosRepository _videoRepository;
        private readonly IHostApplicationLifetime _appLifetime;

        public VideosController(IVideosRepository repository, IHostApplicationLifetime appLifetime)
        {
            _videoRepository = repository;
            _appLifetime = appLifetime;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<IVideo>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<IVideo>>> GetAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await _videoRepository.GetAll(cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IVideo))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IVideo>> GetSingleAsync(Guid id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await _videoRepository.GetVideo(id, cancellationToken);

            if(result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(IVideo))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IVideo>> PostAsync([FromBody] Video video, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (video == null) return BadRequest();

            var result = await _videoRepository.PostVideo(video, cancellationToken);
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.Created)
            {
                Content = new StringContent(JsonConvert.SerializeObject(result), Encoding.UTF8, "application/json"),
                Headers =
                {
                    Location = new Uri($"api/videos/{result.Id}", UriKind.Relative)
                }
            };
            return new ObjectResult(result) { StatusCode = (int)HttpStatusCode.Created };
        }

        [HttpPatch]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IVideo))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IVideo>> PatchAsync([FromBody] Video video, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if(video == null) return BadRequest();

            if (await _videoRepository.IsPresentInDatabase(video.Id, cancellationToken))
            {

                var result = await _videoRepository.UpdateVideo(video, cancellationToken);

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

            if(await _videoRepository.IsPresentInDatabase(id, cancellationToken))
            {
                await _videoRepository.DeleteVideo(id, cancellationToken);

                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }
    }
}
