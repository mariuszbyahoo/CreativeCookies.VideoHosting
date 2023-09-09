using CreativeCookies.VideoHosting.API.Utils;
using CreativeCookies.VideoHosting.Contracts.Services;
using CreativeCookies.VideoHosting.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CreativeCookies.VideoHosting.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ErrorController : ControllerBase
    {
        private readonly IErrorLogsService _srv;

        public ErrorController(IErrorLogsService srv)
        {
            _srv = srv;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var res = _srv.GetErrorLogs();
            return Ok(res);
        }

        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> LogNewError([FromBody] ErrorLogDto errorLogRequest)
        {
            // Check if the errorLog property is null or empty
            if (string.IsNullOrEmpty(errorLogRequest.Log))
            {
                return BadRequest("ErrorLog cannot be null or empty.");
            }

            var newError = await _srv.AddNewLog(errorLogRequest.Log);

            if (newError != null)
            {
                return new CreatedObjectResult(newError);
            }
            else
            {
                throw new InvalidOperationException("IErrorLogsRepository returned null with valid input");
            }
        }
    }
}
