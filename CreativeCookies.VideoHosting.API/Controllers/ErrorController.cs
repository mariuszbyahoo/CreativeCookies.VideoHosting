using CreativeCookies.VideoHosting.API.Utils;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CreativeCookies.VideoHosting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ErrorController : ControllerBase
    {
        private readonly IErrorLogsRepository _repo;
        private AppDbContext _context;

        public ErrorController(AppDbContext context, IErrorLogsRepository repo)
        {
            _context = context;
            _repo = repo;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var res = _context.ClientErrors.ToList();
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

            var newError = await _repo.LogNewError(errorLogRequest.Log);

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
