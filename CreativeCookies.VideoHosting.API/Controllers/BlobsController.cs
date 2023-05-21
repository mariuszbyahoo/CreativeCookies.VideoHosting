using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Azure.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CreativeCookies.VideoHosting.Domain.DTOs;
using CreativeCookies.VideoHosting.Contracts.Repositories;

namespace CreativeCookies.VideoHosting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlobsController : ControllerBase
    {
        private readonly IFilmsRepository _filmsRepository;

        public BlobsController(IFilmsRepository filmsRepository)
        {
            _filmsRepository = filmsRepository;
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
    }
}
