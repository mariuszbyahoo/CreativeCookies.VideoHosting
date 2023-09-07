using CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe;
using CreativeCookies.VideoHosting.Contracts.Services.Stripe;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace CreativeCookies.VideoHosting.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IStripeProductsService _stripeProductsService;
        private readonly ISubscriptionPlanService _subscriptionPlanService;
        private readonly IConnectAccountsService _connectAccountsSrv;
        private readonly IStripeOnboardingService _stripeService;
        private readonly ILogger<StripeWebhookController> _logger;
        private readonly IConfiguration _configuration;

        public StripeWebhookController(IConnectAccountsService connectAccountsSrv, IStripeOnboardingService stripeService, 
            IStripeProductsService stripeProductsService, ISubscriptionPlanService subscriptionPlanService, 
            IConfiguration configuration, ILogger<StripeWebhookController> logger)
        {
            _stripeProductsService = stripeProductsService;
            _subscriptionPlanService = subscriptionPlanService;
            _stripeService = stripeService;
            _connectAccountsSrv = connectAccountsSrv;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("")]
        public async Task<IActionResult> Endpoint()
        {
            _logger.LogInformation("StripeWebhook called");
            string endpointSecret = _configuration.GetValue<string>("WebhookEndpointSecret");

            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"],
                    endpointSecret);

                if (stripeEvent.Type == Events.ProductCreated || stripeEvent.Type == Events.ProductUpdated)
                {
                    _logger.LogInformation($"StripeWebhook with event type of {Enum.GetName(typeof(Events), stripeEvent)}");
                    var product = stripeEvent.Data.Object as Product;
                    if (product != null)
                    {
                        await _stripeProductsService.UpsertStripeProduct(product.Name, product.Description);
                        _logger.LogInformation($"StripeWebhook product upserted: {product.ToJson()}");
                    }
                }
                else if (stripeEvent.Type == Events.ProductDeleted)
                {
                    _logger.LogInformation($"StripeWebhook with event type of {Enum.GetName(typeof(Events), stripeEvent)}");
                    var product = stripeEvent.Data.Object as Product;
                    if (product != null)
                    {
                        await _stripeProductsService.DeleteStripeProduct(product.Id);
                        await _subscriptionPlanService.DeleteSubscriptionPlan(product.Id);
                        _logger.LogInformation($"StripeWebhook product deleted: {product.ToJson()}");
                    }
                }
                else if (stripeEvent.Type == Events.AccountUpdated)
                {
                    _logger.LogInformation($"StripeWebhook with event type of {Enum.GetName(typeof(Events), stripeEvent)}");
                    var account = stripeEvent.Data.Object as Account;
                    await _connectAccountsSrv.EnsureSaved(account.Id);
                    _logger.LogInformation($"StripeWebhook account updated: {account.ToJson()}");
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
            _logger.LogInformation("StripeWebhook returns 200");
            return Ok();
        }
    }
}
