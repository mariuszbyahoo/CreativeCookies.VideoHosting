using Azure.Core;
using CreativeCookies.StripeEvents.Contracts;
using CreativeCookies.StripeEvents.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;
using Stripe;

namespace CreativeCookies.StripeEvents.Services
{
    public class StripeEventsDistributor : IStripeEventsDistributor
    {
        private readonly IDeployedInstancesService _service;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly string _tableStorageAccountKey;

        public StripeEventsDistributor(IConfiguration configuration, ILogger<StripeEventsDistributor> logger, IDeployedInstancesService service)
        {
            _logger = logger;
            _service = service;
            _configuration = configuration;
            _tableStorageAccountKey = _configuration.GetValue<string>("TableStorageAccountKey");
        }

        public async Task RedirectEvent(StripeEventDTO stripeEventDto)
        {
            string endpointSecret = _configuration.GetValue<string>("WebhookEndpointSecret");
            _logger.LogInformation("\nStarting event redirection process");
            try
            { // This service is vulnerable for timeouts. User story #168
                var stripeEvent = EventUtility.ConstructEvent(stripeEventDto.JsonRequestBody,
                stripeEventDto.StripeSignature, endpointSecret);

                if (stripeEvent.Type == Events.ProductCreated || stripeEvent.Type == Events.ProductUpdated)
                {
                    var accountId = stripeEvent.Account;
                    var apiDomain = await _service.GetDestinationUrlByAccountId(accountId, _tableStorageAccountKey);
                    var targetUrl = $"https://{apiDomain}";
                    _logger.LogInformation($"\nEvent type {stripeEvent.Type}");

                    await RedirectEvent(targetUrl, stripeEventDto.JsonRequestBody, stripeEventDto.StripeSignature);
                }
                else if (stripeEvent.Type == Events.ProductDeleted)
                {
                    var accountId = stripeEvent.Account;
                    var apiDomain = await _service.GetDestinationUrlByAccountId(accountId, _tableStorageAccountKey);
                    string targetUrl = $"https://{apiDomain}";
                    _logger.LogInformation($"\nEvent type {stripeEvent.Type}");

                    await RedirectEvent(targetUrl, stripeEventDto.JsonRequestBody, stripeEventDto.StripeSignature);
                }
                else if (stripeEvent.Type == Events.AccountUpdated)
                {
                    var account = stripeEvent.Data.Object as Stripe.Account;
                    if (account == null) _logger.LogError("event.Data.Object is not a Stripe.Account");
                    else
                    {
                        var tableResponse = await _service.UpdateAccountId(account.Email, account.Id, _tableStorageAccountKey);
                        var apiDomain = await _service.GetDestinationUrlByEmail(account.Email, _tableStorageAccountKey);
                        var targetUrl = $"https://{apiDomain}";
                        _logger.LogInformation($"\nEvent type {stripeEvent.Type}");

                        await RedirectEvent(targetUrl, stripeEventDto.JsonRequestBody, stripeEventDto.StripeSignature);
                    }
                }
                else if (stripeEvent.Type == Events.InvoicePaymentSucceeded)
                {
                    var accountId = stripeEvent.Account;
                    var apiDomain = await _service.GetDestinationUrlByAccountId(accountId, _tableStorageAccountKey);
                    var targetUrl = $"https://{apiDomain}";
                    _logger.LogInformation($"\nEvent type {stripeEvent.Type}");

                    await RedirectEvent(targetUrl, stripeEventDto.JsonRequestBody, stripeEventDto.StripeSignature);
                }
                else if (stripeEvent.Type == Events.CheckoutSessionCompleted)
                {
                    var accountId = stripeEvent.Account;
                    var apiDomain = await _service.GetDestinationUrlByAccountId(accountId, _tableStorageAccountKey);
                    var targetUrl = $"https://{apiDomain}";
                    _logger.LogInformation($"\nEvent type {stripeEvent.Type}");

                    await RedirectEvent(targetUrl, stripeEventDto.JsonRequestBody, stripeEventDto.StripeSignature);
                }
                else if (stripeEvent.Type == Events.ChargeRefunded)
                {
                    var accountId = stripeEvent.Account;
                    var apiDomain = await _service.GetDestinationUrlByAccountId(accountId, _tableStorageAccountKey);
                    var targetUrl = $"https://{apiDomain}";
                    _logger.LogInformation($"\nEvent type {stripeEvent.Type}");

                    await RedirectEvent(targetUrl, stripeEventDto.JsonRequestBody, stripeEventDto.StripeSignature);
                }
                else if (stripeEvent.Type == Events.CustomerSubscriptionDeleted)
                {
                    var accountId = stripeEvent.Account;
                    var apiDomain = await _service.GetDestinationUrlByAccountId(accountId, _tableStorageAccountKey);
                    var targetUrl = $"https://{apiDomain}";
                    _logger.LogInformation($"\nEvent type {stripeEvent.Type}");

                    await RedirectEvent(targetUrl, stripeEventDto.JsonRequestBody, stripeEventDto.StripeSignature);
                }
                else if (stripeEvent.Type == Events.SubscriptionScheduleCanceled)
                {
                    var accountId = stripeEvent.Account;
                    var apiDomain = await _service.GetDestinationUrlByAccountId(accountId, _tableStorageAccountKey);
                    var targetUrl = $"https://{apiDomain}";
                    _logger.LogInformation($"\nEvent type {stripeEvent.Type}");

                    await RedirectEvent(targetUrl, stripeEventDto.JsonRequestBody, stripeEventDto.StripeSignature);
                }
                else
                {
                    _logger.LogWarning("\nUnexpected event type has been received, not redirected");
                }
            }
            catch (StripeException e)
            {
                var exMsg = $"\nException message: {e.Message}, InnerException: {e.InnerException}, StackTrace: {e.StackTrace}, Source: {e.Source}";
                _logger.LogError(exMsg, e );
            }
            catch (Exception e)
            {
                var exMsg = $"\nException message: {e.Message}, InnerException: {e.InnerException}, StackTrace: {e.StackTrace}, Source: {e.Source}";
                _logger.LogError(exMsg, e );
            }
            _logger.LogInformation("\nSuccessfully finished redirection process.");
        }
        private async Task RedirectEvent(string targetUrl, string jsonRequestBody, string stripeSignature)
        {
            var client = new RestClient(targetUrl);
            var request = new RestRequest("StripeWebhook", Method.Post);

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Stripe-Signature", stripeSignature);
            request.AddBody(jsonRequestBody, RestSharp.ContentType.Plain);

            var response = await client.ExecuteAsync(request);

            _logger.LogInformation($"\nRedirection result status code: {response.StatusCode}");
        }
    }
}
