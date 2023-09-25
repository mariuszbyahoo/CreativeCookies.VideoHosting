using CreativeCookies.StripeEvents.RedistributionService.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;
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
        private readonly IConfiguration _configuration;
        private readonly string _tableStorageAccountKey;

        public EventsRedistributionController(IDeployedInstancesService service, IConfiguration configuration)
        {
            _service = service;
            _configuration = configuration;
            _tableStorageAccountKey = _configuration.GetValue<string>("TableStorageAccountKey");
        }

        //[HttpGet("")]
        //public async Task<IActionResult> GetApiUrl(string accountId)
        //{
        //    try
        //    {
        //        var res = await _service.GetDestinationUrlByAccountId(accountId, _tableStorageAccountKey);
        //        return Ok(res);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        [HttpPost("")]
        public async Task<IActionResult> ProcessEvent()
        {
            string endpointSecret = _configuration.GetValue<string>("WebhookEndpointSecret");
            var msg = "Check logs for an exception - event has not been forwarded ";
            var jsonRequestBody = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(jsonRequestBody,
                Request.Headers["Stripe-Signature"],
                endpointSecret);

                if (stripeEvent.Type == Events.ProductCreated || stripeEvent.Type == Events.ProductUpdated)
                {
                    var accountId = stripeEvent.Account;
                    var apiDomain = await _service.GetDestinationUrlByAccountId(accountId, _tableStorageAccountKey);
                    var targetUrl = $"https://{apiDomain}";

                    return await RedirectEvent(targetUrl, jsonRequestBody, Request.Headers["Stripe-Signature"]);
                }
                else if (stripeEvent.Type == Events.ProductDeleted)
                {
                    var accountId = stripeEvent.Account;
                    var apiDomain = await _service.GetDestinationUrlByAccountId(accountId, _tableStorageAccountKey);
                    string targetUrl = $"https://{apiDomain}";

                    return await RedirectEvent(targetUrl, jsonRequestBody, Request.Headers["Stripe-Signature"]);
                }
                else if (stripeEvent.Type == Events.AccountUpdated)
                {
                    var account = stripeEvent.Data.Object as Stripe.Account;
                    if (account == null) return BadRequest("event.Data.Object is not a Stripe.Account");
                    var tableResponse = await _service.UpdateAccountId(account.Email, account.Id, _tableStorageAccountKey);
                    var apiDomain = await _service.GetDestinationUrlByEmail(account.Email, _tableStorageAccountKey);
                    var targetUrl = $"https://{apiDomain}";

                    return await RedirectEvent(targetUrl, jsonRequestBody, Request.Headers["Stripe-Signature"]);
                }
                else if (stripeEvent.Type == Events.InvoicePaymentSucceeded)
                {
                    var accountId = stripeEvent.Account;
                    var apiDomain = await _service.GetDestinationUrlByAccountId(accountId, _tableStorageAccountKey);
                    var targetUrl = $"https://{apiDomain}";

                    return await RedirectEvent(targetUrl, jsonRequestBody, Request.Headers["Stripe-Signature"]);
                }
                else if (stripeEvent.Type == Events.ChargeRefunded)
                {
                    var accountId = stripeEvent.Account;
                    var apiDomain = await _service.GetDestinationUrlByAccountId(accountId, _tableStorageAccountKey);
                    var targetUrl = $"https://{apiDomain}";

                    return await RedirectEvent(targetUrl, jsonRequestBody, Request.Headers["Stripe-Signature"]);
                }
                else if (stripeEvent.Type == Events.CustomerSubscriptionDeleted)
                {
                    var accountId = stripeEvent.Account;
                    var apiDomain = await _service.GetDestinationUrlByAccountId(accountId, _tableStorageAccountKey);
                    var targetUrl = $"https://{apiDomain}";

                    return await RedirectEvent(targetUrl, jsonRequestBody, Request.Headers["Stripe-Signature"]);
                }
                else if (stripeEvent.Type == Events.SubscriptionScheduleCanceled)
                {
                    var accountId = stripeEvent.Account;
                    var apiDomain = await _service.GetDestinationUrlByAccountId(accountId, _tableStorageAccountKey);
                    var targetUrl = $"https://{apiDomain}";

                    return await RedirectEvent(targetUrl, jsonRequestBody, Request.Headers["Stripe-Signature"]);
                }
                else
                {
                    return NoContent();
                }
            }
            catch (StripeException e)
            {
                msg = $"{msg} Exception message: {e.Message}, InnerException: {e.InnerException}, StackTrace: {e.StackTrace}, Source: {e.Source}";
                return BadRequest(msg);
            }
            catch (Exception e)
            {
                msg = $"{msg} Exception message: {e.Message}, InnerException: {e.InnerException}, StackTrace: {e.StackTrace}, Source: {e.Source}";
            }
            return Ok(msg);
        }
        private async Task<IActionResult> RedirectEvent(string targetUrl, string jsonRequestBody, string stripeSignature)
        {
            var client = new RestClient(targetUrl);
            var request = new RestRequest("StripeWebhook", Method.Post);

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Stripe-Signature", stripeSignature);
            request.AddBody(jsonRequestBody, ContentType.Plain);

            var response = await client.ExecuteAsync(request);

            return Ok($"Redirection result status code: {response.StatusCode}");
        }
    }
}
