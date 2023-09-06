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
        private readonly ILogger<EventsRedistributionController> _logger;
        private readonly IConfiguration _configuration;

        public EventsRedistributionController(ILogger<EventsRedistributionController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
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
