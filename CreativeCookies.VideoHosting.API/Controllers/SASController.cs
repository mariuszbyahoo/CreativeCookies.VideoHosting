using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using CreativeCookies.VideoHosting.Contracts.Infrastructure.Azure;

namespace CreativeCookies.VideoHosting.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SASController : ControllerBase
    {
        private readonly ISasTokenService _sasTokenService;
        private readonly ILogger<SASController> _logger;
        public SASController(ISasTokenService sasTokenService, ILogger<SASController> logger)
        {
            _sasTokenService = sasTokenService;
            _logger = logger;
        }

        [HttpGet("filmsList")]
        public IActionResult GetSasTokenForContainer()
        {
            try
            {
                var res = _sasTokenService.GetSasTokenForContainer("films");
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
                var res = _sasTokenService.GetSasTokenForFilm(blobTitle);
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
                var res = _sasTokenService.GetSasTokenForFilmUpload(blobTitle);
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
                var res = _sasTokenService.GetSasTokenForThumbnail(blobTitle);
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
                var res = _sasTokenService.GetSasTokenForThumbnailUpload(blobTitle);
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
