using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Azure.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CreativeCookies.VideoHosting.Domain.Endpoints;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace CreativeCookies.VideoHosting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SASController : ControllerBase
    {
        private readonly ISasTokenRepository _sasTokenRepository;

        public SASController(ISasTokenRepository sasTokenRepository)
        {
            _sasTokenRepository = sasTokenRepository;
        }

        [HttpGet("filmsList")]
        public IActionResult GetSasTokenForContainer()
        {
            var res = _sasTokenRepository.GetSasTokenForContainer("films");
            return Ok(res);
        }

        [HttpGet("film/{blobTitle}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetSasTokenForFilm(string blobTitle)
        {
            if (string.IsNullOrEmpty(blobTitle))
            {
                return BadRequest($"Field: string blobTitle is mandatory!");
            }
            var res = _sasTokenRepository.GetSasTokenForFilm(blobTitle);
            return Ok(res);
        }

        [HttpGet("film-upload/{blobTitle}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetSasTokenForFilmUpload(string blobTitle)
        {
            if (string.IsNullOrEmpty(blobTitle))
            {
                return BadRequest($"Field: string blobTitle is mandatory!");
            }
            var res = _sasTokenRepository.GetSasTokenForFilmUpload(blobTitle);
            return Ok(res);
        }

        [HttpGet("thumbnail/{blobTitle}")]
        public IActionResult GetSasTokenForThumbnail(string blobTitle)
        {
            if (string.IsNullOrEmpty(blobTitle))
            {
                return BadRequest($"Field: string blobTitle is mandatory!");
            }
            var res = _sasTokenRepository.GetSasTokenForThumbnail(blobTitle);
            return Ok(res);
        }

        [HttpGet("thumbnail-upload/{blobTitle}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetSasTokenForThumbnailUpload(string blobTitle)
        {
            if (string.IsNullOrEmpty(blobTitle))
            {
                return BadRequest($"Field: string blobTitle is mandatory!");
            }
            var res = _sasTokenRepository.GetSasTokenForThumbnailUpload(blobTitle);
            return Ok(res);
        }
    }
}
