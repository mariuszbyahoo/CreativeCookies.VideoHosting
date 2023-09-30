using CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.Services.IdP;
using CreativeCookies.VideoHosting.Contracts.Services.Stripe;
using CreativeCookies.VideoHosting.DTOs.OAuth;
using CreativeCookies.VideoHosting.Infrastructure.Azure.Wrappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
        private readonly ILogger<StripeWebhookController> _logger;
        private readonly StripeWebhookSigningKeyWrapper _wrapper;
        private readonly IMyHubUserManager _userManager;
        private readonly IUsersRepository _userRepo;

        public StripeWebhookController(IConnectAccountsService connectAccountsSrv, 
            IStripeProductsService stripeProductsService, ISubscriptionPlanService subscriptionPlanService, 
            ILogger<StripeWebhookController> logger, StripeWebhookSigningKeyWrapper wrapper, IMyHubUserManager userManager, IUsersRepository userRepo)
        {
            _stripeProductsService = stripeProductsService;
            _subscriptionPlanService = subscriptionPlanService;
            _connectAccountsSrv = connectAccountsSrv;
            _userManager= userManager;
            _userRepo = userRepo;
            _logger = logger;
            _wrapper = wrapper;
        }

        [HttpPost("")]
        public async Task<IActionResult> Endpoint()
        {
            _logger.LogInformation("StripeWebhook called");
            string endpointSecret = _wrapper.Value;

            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"],
                    endpointSecret);

                if (stripeEvent.Type == Events.ProductCreated || stripeEvent.Type == Events.ProductUpdated)
                {
                    _logger.LogInformation($"StripeWebhook with event type of {stripeEvent.Type}");
                    var product = stripeEvent.Data.Object as Product;
                    if (product != null)
                    {
                        await _subscriptionPlanService.UpsertSubscriptionPlan(new VideoHosting.DTOs.Stripe.SubscriptionPlanDto(product.Id, product.Name, product.Description));
                        _logger.LogInformation($"StripeWebhook product upserted: {product.ToJson()}");
                    }
                }
                else if (stripeEvent.Type == Events.ProductDeleted)
                {
                    _logger.LogInformation($"StripeWebhook with event type of {stripeEvent.Type}");
                    var product = stripeEvent.Data.Object as Product;
                    if (product != null)
                    {
                        await _subscriptionPlanService.DeleteSubscriptionPlan(product.Id);
                        _logger.LogInformation($"StripeWebhook product deleted: {product.ToJson()}");
                    }
                }
                else if (stripeEvent.Type == Events.AccountUpdated)
                {
                    _logger.LogInformation($"StripeWebhook with event type of {stripeEvent.Type}");
                    var account = stripeEvent.Data.Object as Account;
                    await _connectAccountsSrv.EnsureSaved(account.Id);
                    _logger.LogInformation($"StripeWebhook account updated: {account.ToJson()}");
                }
                else if (stripeEvent.Type == Events.InvoicePaymentSucceeded)
                {
                    _logger.LogInformation($"StripeWebhook with event type of {stripeEvent.Type}");
                    var invoice = stripeEvent.Data.Object as Invoice;
                    var res = await _userRepo.ChangeSubscriptionEndDateUTC(invoice.CustomerId, invoice.Lines.Data[0].Period.End);
                    if (res) _logger.LogInformation($"SubscriptionEndDateUTC of Stripe Customer id: {invoice.CustomerId} updated to {invoice.Lines.Data[0].Period.End}");
                    else return BadRequest($"Database result of SubscriptionEndDateUTC update was false for customer with id: {invoice.CustomerId}");
                }
                else if (stripeEvent.Type == Events.ChargeRefunded)
                {
                    _logger.LogInformation($"StripeWebhook with event type of {stripeEvent.Type}");
                    var accountId = stripeEvent.Account;
                    var charge = stripeEvent.Data.Object as Charge;
                    var res = await _userRepo.ChangeSubscriptionEndDateUTC(charge.CustomerId, DateTime.UtcNow);
                    if (res) _logger.LogInformation($"SubscriptionEndDateUTC of Stripe Customer id: {charge.CustomerId} updated to {DateTime.UtcNow}");
                    else return BadRequest($"Database result of SubscriptionEndDateUTC update was false for customer with id: {charge.CustomerId}");
                }
                else if (stripeEvent.Type == Events.CustomerSubscriptionDeleted)
                {
                    _logger.LogInformation($"StripeWebhook with event type of {stripeEvent.Type}");
                    var accountId = stripeEvent.Account;
                    var subscription = stripeEvent.Data.Object as Subscription;
                    var res = await _userRepo.ChangeSubscriptionEndDateUTC(subscription.CustomerId, DateTime.UtcNow);
                    if (res) _logger.LogInformation($"SubscriptionEndDateUTC of Stripe Customer id: {subscription.CustomerId} updated to {DateTime.UtcNow}");
                    else return BadRequest($"Database result of SubscriptionEndDateUTC update was false for customer with id: {subscription.CustomerId}");
                }
                else if (stripeEvent.Type == Events.SubscriptionScheduleCanceled)
                {
                    return Ok("TODO: IMPLEMENT SUBSCRIPITON SCHEDULE CANCELED HANDLER!");
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
                return BadRequest($"Stripe exception occured: {e.Message}, {e.InnerException}, {e.StackTrace}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message, e.StackTrace);
                return BadRequest($"Unexpected exception occured: {e.Message}, {e.InnerException}, {e.StackTrace}");
            }
            _logger.LogInformation("StripeWebhook returns 200");
            return Ok();
        }
    }
}
