using CreativeCookies.VideoHosting.App.Data;
using CreativeCookies.VideoHosting.App.Models;
using CreativeCookies.VideoHosting.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CreativeCookies.VideoHosting.App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ErrorController : ControllerBase
    {
        private AppDbContext _context;

        public ErrorController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var res = _context.ClientErrors.ToList();
            return Ok(res);
        }

        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> LogNewError([FromBody] ErrorLogRequest errorLogRequest)
        {
            // Check if the errorLog property is null or empty
            if (string.IsNullOrEmpty(errorLogRequest.ErrorLog))
            {
                return BadRequest("ErrorLog cannot be null or empty.");
            }

            var newError = new ClientError() { Id = Guid.NewGuid(), ErrorLog = errorLogRequest.ErrorLog };

            await _context.AddAsync(newError);
            await _context.SaveChangesAsync();
            return new CreatedObjectResult(newError);
        }
    }
}
