using Azure.Messaging.ServiceBus;
using CreativeCookies.StripeEvents.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CreativeCookies.StripeEvents.MailboxService.Controllers
{
    [Route("")]
    [ApiController]
    public class MailboxController : ControllerBase
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly string _queueName = "stripe_events_queue";  

        public MailboxController(IConfiguration configuration)
        {
            _serviceBusClient = new ServiceBusClient(configuration.GetValue<string>("ServiceBusConnectionString"));
        }

        //[HttpGet("")]
        //public IActionResult GetStatus()
        //{
        //    return Ok("Mailbox running!");
        //}

        [HttpPost("")]
        public async Task<IActionResult> ReceiveStripeEvent()
        {
            string jsonRequestBody = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            string stripeSignature = Request.Headers["Stripe-Signature"];

            var eventRequestDTO = new StripeEventDTO() { JsonRequestBody = jsonRequestBody, StripeSignature = stripeSignature };

            var serializedRequestData = JsonSerializer.Serialize(eventRequestDTO);

            await SendMessageToQueue(serializedRequestData);

            return Ok("Event received.");
        }

        private async Task SendMessageToQueue(string messageBody)
        {
            ServiceBusSender sender = _serviceBusClient.CreateSender(_queueName);
            ServiceBusMessage message = new ServiceBusMessage(messageBody);

            await sender.SendMessageAsync(message);
        }
    }
}
