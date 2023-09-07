using CreativeCookies.StripeEvents.RedistributionService.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using System.Security.Principal;
using System.Text;

namespace CreativeCookies.StripeEvents.RedistributionService.Controllers
{
    [Route("")]
    [ApiController]
    public class EventsRedistributionController : ControllerBase
    {
        private readonly IDeployedInstancesService _service;
        private readonly ILogger<EventsRedistributionController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _tableStorageAccountKey;

        public EventsRedistributionController(IDeployedInstancesService service, ILogger<EventsRedistributionController> logger, IConfiguration configuration)
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

            var jsonRequestBody = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(jsonRequestBody,
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
                    var account = (Account)stripeEvent.Data.Object;
                    var tableResponse = await _service.InsertAccountId(account.Email, account.Id, _tableStorageAccountKey);
                    _logger.LogInformation($"Azure Table Storage response: {System.Text.Json.JsonSerializer.Serialize(tableResponse)}");
                    string targetUrl = $"https://{await _service.GetDestinationUrlByEmail(account.Email, _tableStorageAccountKey)}/StripeWebhook"; 

                    using (HttpClient httpClient = new HttpClient())
                    {
                        var request = new HttpRequestMessage(HttpMethod.Post, targetUrl)
                        {
                            Content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json")
                        };

                        var response = await httpClient.SendAsync(request);

                        if (response.IsSuccessStatusCode)
                        {
                            _logger.LogInformation($"Successfully forwarded event {stripeEvent.Id} to {targetUrl}");
                            return Ok();
                        }
                        else
                        {
                            _logger.LogWarning($"Failed to forward event {stripeEvent.Id} to {targetUrl}");
                            return BadRequest();
                        }
                    }
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
