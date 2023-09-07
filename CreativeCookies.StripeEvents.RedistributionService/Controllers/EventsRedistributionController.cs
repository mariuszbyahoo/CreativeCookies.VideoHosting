using CreativeCookies.StripeEvents.RedistributionService.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.FinancialConnections;
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

        [HttpPost("")]
        public async Task<IActionResult> ProcessEvent()
        {
            _logger.LogInformation("EventsRedistributionController called");
            string endpointSecret = "whsec_5a47597a9ce53e2107dba3f79794a4853847ed41c8281625895196654c06271a";//_configuration.GetValue<string>("WebhookEndpointSecret");

            var jsonRequestBody = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(jsonRequestBody,
                Request.Headers["Stripe-Signature"],
                endpointSecret);

                if (stripeEvent.Type == Events.ProductCreated || stripeEvent.Type == Events.ProductUpdated)
                {
                    _logger.LogInformation($"EventsRedistributionController with event type of {Enum.GetName(typeof(Events), stripeEvent)}");
                    var accountId = stripeEvent.Account;
                    var apiDomain = _service.GetDestinationUrlByAccountId(accountId, _tableStorageAccountKey);
                    string targetUrl = $"https://{apiDomain}/StripeWebhook";

                    return await RedirectEvent(targetUrl, jsonRequestBody, stripeEvent.Id);
                }
                else if (stripeEvent.Type == Events.ProductDeleted)
                {
                    _logger.LogInformation($"EventsRedistributionController with event type of {Enum.GetName(typeof(Events), stripeEvent)}");
                    var accountId = stripeEvent.Account;
                    var apiDomain = _service.GetDestinationUrlByAccountId(accountId, _tableStorageAccountKey);
                    string targetUrl = $"https://{apiDomain}/StripeWebhook";

                    return await RedirectEvent(targetUrl, jsonRequestBody, stripeEvent.Id);
                }
                else if (stripeEvent.Type == Events.AccountUpdated)
                {
                    _logger.LogInformation($"EventsRedistributionController with event type of {Enum.GetName(typeof(Events), stripeEvent)}");
                    var account = (Stripe.Account)stripeEvent.Data.Object;
                    var tableResponse = await _service.InsertAccountId(account.Email, account.Id, _tableStorageAccountKey);
                    _logger.LogInformation($"Azure Table Storage response: {System.Text.Json.JsonSerializer.Serialize(tableResponse)}");
                    var apiDomain = await _service.GetDestinationUrlByEmail(account.Email, _tableStorageAccountKey);
                    string targetUrl = $"https://{apiDomain}/StripeWebhook";

                    return await RedirectEvent(targetUrl, jsonRequestBody, stripeEvent.Id);
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

        private async Task<IActionResult> RedirectEvent(string targetUrl, string jsonRequestBody, string eventId)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, targetUrl)
                {
                    Content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json")
                };

                var response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Successfully forwarded event {eventId} to {targetUrl}");
                    return Ok();
                }
                else
                {
                    _logger.LogWarning($"Failed to forward event {eventId} to {targetUrl}");
                    return BadRequest();
                }
            }
        }
    }
}
