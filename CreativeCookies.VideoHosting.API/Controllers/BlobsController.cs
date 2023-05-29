using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Azure.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.DTOs;
using CreativeCookies.VideoHosting.Domain.DTOs;
using CreativeCookies.VideoHosting.API.Utils;
using System.Security.Permissions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

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
        [Route("getMetadata")]
        public async Task<IActionResult> GetVideoMetadata([FromQuery] string Id)
        {
            Guid videoId;
            if(Guid.TryParse(Id, out videoId))
            {
                var res = await _filmsRepository.GetVideoMetadata(videoId);
                if (res != null)
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

        [HttpPatch]
        [Route("editMetadata")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> EditVideoMetadata([FromBody] VideoMetadata metadata)
        {
            var res = await _filmsRepository.EditVideoMetadata(metadata);
            if (res != null)
            {
                return Ok(res);
            }
            else return NotFound("Video with such an ID has not been found");
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> SaveVideoMetadata([FromBody] VideoMetadata metadata)
        {
            try
            {
                var res = await _filmsRepository.SaveVideoMetadata(metadata);
                if (res != null)
                {
                    return new CreatedObjectResult(res);
                }
                else { return BadRequest("SaveVideoMetadata returned null!"); };
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError($"Unexpected error occured on attempt to save video metadata: {ex.Message} ; {ex.InnerException} ; {ex.Source}", ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Route("deleteVideo")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteVideo(string Id)
        {
            Guid videoId;
            if (Guid.TryParse(Id, out videoId))
                await _filmsRepository.DeleteVideoBlobWithMetadata(videoId);
            else return BadRequest("Id supplied in an argument is not a valid GUID");
            return NoContent();
        }
    }
}
