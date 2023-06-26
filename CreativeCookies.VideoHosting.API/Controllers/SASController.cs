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
        private readonly ILogger<SASController> _logger;
        public SASController(ISasTokenRepository sasTokenRepository, ILogger<SASController> logger)
        {
            _sasTokenRepository = sasTokenRepository;
            _logger = logger;
        }

        [HttpGet("filmsList")]
        public IActionResult GetSasTokenForContainer()
        {
            try
            {
                var res = _sasTokenRepository.GetSasTokenForContainer("films");
                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected exception occured during action: SASController.GetSasTokenForContainer: ex: {ex.ToString()}, ex.Message: {ex.Message}, ex.InnerException: {ex.InnerException}, ex.Source: {ex.Source}");
                return StatusCode(505, "Internal Server Error");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="blobTitle">blob's title WITH video file format eg. mp4</param>
        /// <returns></returns>
        [HttpGet("film/{blobTitle}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin,ADMIN,subscriber,SUBSCRIBER")]
        public IActionResult GetSasTokenForFilm(string blobTitle)
        {
            try
            {
                if (string.IsNullOrEmpty(blobTitle))
                {
                    return BadRequest($"Field: string blobTitle is mandatory!");
                }
                var res = _sasTokenRepository.GetSasTokenForFilm(blobTitle);
                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected exception occured during action: SASController.GetSasTokenForFilm: ex: {ex.ToString()}, ex.Message: {ex.Message}, ex.InnerException: {ex.InnerException}, ex.Source: {ex.Source}");
                return StatusCode(505, "Internal Server Error");
            }
        }

        [HttpGet("film-upload/{blobTitle}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin,ADMIN")]
        public IActionResult GetSasTokenForFilmUpload(string blobTitle)
        {
            try 
            { 
                if (string.IsNullOrEmpty(blobTitle))
                {
                    return BadRequest($"Field: string blobTitle is mandatory!");
                }
                var res = _sasTokenRepository.GetSasTokenForFilmUpload(blobTitle);
                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected exception occured during action: SASController.GetSasTokenForFilmUpload: ex: {ex.ToString()}, ex.Message: {ex.Message}, ex.InnerException: {ex.InnerException}, ex.Source: {ex.Source}");
                return StatusCode(505, "Internal Server Error");
            }
        }

        [HttpGet("thumbnail/{blobTitle}")]
        public IActionResult GetSasTokenForThumbnail(string blobTitle)
        {
            try { 
                if (string.IsNullOrEmpty(blobTitle))
                {
                    return BadRequest($"Field: string blobTitle is mandatory!");
                }
                var res = _sasTokenRepository.GetSasTokenForThumbnail(blobTitle);
                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected exception occured during action: SASController.GetSasTokenForThumbnail: ex: {ex.ToString()}, ex.Message: {ex.Message}, ex.InnerException: {ex.InnerException}, ex.Source: {ex.Source}");
                return StatusCode(505, "Internal Server Error");
            }
        }

        [HttpGet("thumbnail-upload/{blobTitle}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin,ADMIN")]
        public IActionResult GetSasTokenForThumbnailUpload(string blobTitle)
        {
            try
            {
                if (string.IsNullOrEmpty(blobTitle))
                {
                    return BadRequest($"Field: string blobTitle is mandatory!");
                }
                var res = _sasTokenRepository.GetSasTokenForThumbnailUpload(blobTitle);
                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected exception occured during action: SASController.GetSasTokenForThumbnailUpload: ex: {ex.ToString()}, ex.Message: {ex.Message}, ex.InnerException: {ex.InnerException}, ex.Source: {ex.Source}");
                return StatusCode(505, "Internal Server Error");
            }
        }
    }
}
