using CreativeCookies.StripeEvents.RedistributionService.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using System.Security.Principal;

namespace CreativeCookies.StripeEvents.RedistributionService.Controllers
{
    [Route("")]
    [ApiController]
    public class EventsRedistributionController : ControllerBase
    {
        private readonly ITargetUrlService _service;
        private readonly ILogger<EventsRedistributionController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _tableStorageAccountKey;

        public EventsRedistributionController(ITargetUrlService service, ILogger<EventsRedistributionController> logger, IConfiguration configuration)
        {
            _service = service;
            _logger = logger;
            _configuration = configuration;
            _tableStorageAccountKey = _configuration.GetValue<string>("TableStorageAccountKey");
        }

        [HttpGet("")]
        public async Task<IActionResult> GetDestinationUrl(string adminEmail)
        {
            var res = await _service.GetDestinationUrlByEmail(adminEmail, _tableStorageAccountKey);
            return Ok(res);
        }

        [HttpPost("")]
        public async Task<IActionResult> ProcessEvent()
        {
            _logger.LogInformation("EventsRedistributionController called");
            string endpointSecret = _configuration.GetValue<string>("WebhookEndpointSecret");

            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                Request.Headers["Stripe-Signature"],
                endpointSecret);

                if (stripeEvent.Type == Events.ProductCreated || stripeEvent.Type == Events.ProductUpdated)
                {
                    _logger.LogInformation($"EventsRedistributionController with event type of {Enum.GetName(typeof(Events), stripeEvent)}");
                    // HACK: TODO
                }
                else if (stripeEvent.Type == Events.ProductDeleted)
                {
                    _logger.LogInformation($"EventsRedistributionController with event type of {Enum.GetName(typeof(Events), stripeEvent)}");
                    // HACK: TODO
                }
                else if (stripeEvent.Type == Events.AccountUpdated)
                {
                    _logger.LogInformation($"EventsRedistributionController with event type of {Enum.GetName(typeof(Events), stripeEvent)}");
                    // HACK: TODO
                    // 1. Update table record with account_id
                    // 2. Redirect this request unchanged towards returned ApiUrl/StripeWebhook
                }
                else
                {
                    _logger.LogWarning($"Unexpected Stripe event's type: {stripeEvent.ToJson()}");
                    return BadRequest();
                }
            }
            catch (StripeException e)
            {
                _logger.LogError(e, e.Message);
                return BadRequest("Stripe exception occured");
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message, e.StackTrace);
            }
            _logger.LogInformation("EventsRedistributionController returns 200");
            return Ok();
        }
    }
}
