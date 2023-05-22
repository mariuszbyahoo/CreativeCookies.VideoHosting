using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Azure.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.DTOs;
using CreativeCookies.VideoHosting.Domain.DTOs;

namespace CreativeCookies.VideoHosting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlobsController : ControllerBase
    {
        private readonly IFilmsRepository _filmsRepository;
        private ILogger<BlobsController> _logger;

        public BlobsController(IFilmsRepository filmsRepository, ILogger<BlobsController> logger)
        {
            _filmsRepository = filmsRepository;
            _logger = logger;
        }

        [HttpGet]
        [Route("films")]
        public async Task<IActionResult> GetFilms([FromQuery] string search = "", int pageNumber = 1, int pageSize = 24)
        {
            var res = await _filmsRepository.GetFilmsPaginatedResult(search, pageNumber, pageSize);

            return Ok(res);
        }

        [HttpGet]
        [Route("storageUrl")]
        public async Task<IActionResult> GetBlobUrl([FromQuery] string Id)
        {
            Guid videoId;
            if(Guid.TryParse(Id, out videoId))
            {
                var res = await _filmsRepository.GetBlobUrl(videoId);
                if (res != null && !res.BlobUrl.Equals("NOT_FOUND_IN_REPO"))
                {
                    return Ok(res);
                }
                else
                {
                    return NotFound("Video has not been found");
                }
            }
            else
            {
                return BadRequest("Id should be a valid GUID");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveVideoMetadata([FromBody] VideoMetadata metadata)
        {
            try
            {
                await _filmsRepository.SaveVideoMetadata(metadata);
                return Ok();
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError($"Unexpected error occured on attempt to save video metadata: {ex.Message} ; {ex.InnerException} ; {ex.Source}", ex);
                return BadRequest(ex.Message);
            }
        }
    }
}
