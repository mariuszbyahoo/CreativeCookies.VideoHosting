using CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.Services.IdP;
using CreativeCookies.VideoHosting.Contracts.Services.Stripe;
using CreativeCookies.VideoHosting.DTOs.OAuth;
using CreativeCookies.VideoHosting.Infrastructure.Azure.Wrappers;
using Hangfire;
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
        private readonly ICheckoutService _checkoutService;
        private readonly ILogger<StripeWebhookController> _logger;
        private readonly StripeWebhookSigningKeyWrapper _wrapper;
        private readonly IMyHubUserManager _userManager;
        private readonly IUsersRepository _userRepo;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public StripeWebhookController(IConnectAccountsService connectAccountsSrv, 
            IStripeProductsService stripeProductsService, ISubscriptionPlanService subscriptionPlanService, 
            ILogger<StripeWebhookController> logger, StripeWebhookSigningKeyWrapper wrapper, 
            IMyHubUserManager userManager, IUsersRepository userRepo, ICheckoutService checkoutService,
            IBackgroundJobClient backgroundJobClient)
        {
            _stripeProductsService = stripeProductsService;
            _subscriptionPlanService = subscriptionPlanService;
            _connectAccountsSrv = connectAccountsSrv;
            _userManager= userManager;
            _userRepo = userRepo;
            _checkoutService = checkoutService; 
            _backgroundJobClient = backgroundJobClient;
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
                    var invoicePeriodEnd = invoice.Lines.Data[0].Period.End;
                    var accessPeriodEnd = new DateTime(invoicePeriodEnd.Year, invoicePeriodEnd.Month, invoicePeriodEnd.Day, 0, 0, 0, DateTimeKind.Utc).AddDays(1);
                    var res = await _userRepo.ChangeSubscriptionDatesUTC(invoice.CustomerId, invoice.Lines.Data[0].Period.Start, accessPeriodEnd);
                    if (res) _logger.LogInformation($"Subscription dates range for a Stripe Customer id: {invoice.CustomerId} updated to {invoice.Lines.Data[0].Period.Start} - {accessPeriodEnd}");
                    else return BadRequest($"Database result of SubscriptionEndDateUTC update was false for customer with id: {invoice.CustomerId}");
                }
                else if (stripeEvent.Type == Events.CheckoutSessionCompleted)
                {
                    try
                    {
                        _logger.LogInformation($"StripeWebhook with event type of {stripeEvent.Type}");
                        var checkoutSession = stripeEvent.Data.Object as Stripe.Checkout.Session;

                        if (checkoutSession.Mode.Equals("payment"))
                        {
                            var paymentIntentService = new PaymentIntentService();
                            var paymentIntent = paymentIntentService.Get(checkoutSession.PaymentIntentId, requestOptions: new RequestOptions() { StripeAccount = stripeEvent.Account });
                            _logger.LogWarning($"Payment intent performed with an ID: {paymentIntent.Id}, methodId: {paymentIntent.PaymentMethodId}, and sourceId: {paymentIntent.SourceId}");
                            var customerService = new CustomerService();
                            var customerOptions = new CustomerUpdateOptions
                            {
                                InvoiceSettings = new CustomerInvoiceSettingsOptions()
                                {
                                    DefaultPaymentMethod = paymentIntent.PaymentMethodId
                                }
                            };
                            Customer customer = customerService.Update(checkoutSession.CustomerId, customerOptions, requestOptions: new RequestOptions() { StripeAccount = stripeEvent.Account });

                            if(customer.StripeResponse.StatusCode == System.Net.HttpStatusCode.OK)
                                _logger.LogInformation($"Default payment method set for customer {checkoutSession.CustomerId}");
                            else
                                _logger.LogError($"Setting payment as default has not respond with 200 due to error {customer.StripeResponse.StatusCode}, requestId: {customer.StripeResponse.RequestId} with paymentIntent data. An ID: {paymentIntent.Id}, methodId: {paymentIntent.PaymentMethodId}, and sourceId: {paymentIntent.SourceId}");

                            // HACK: Adjust to subscription start on 00:00 UTC of the next day.
                            var beginningOfTommorow = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 0, 0, 0, DateTimeKind.Utc).AddDays(1);
                            var subscriptionStartDate = beginningOfTommorow.AddDays(14); 
                            var subscriptionEndDate = beginningOfTommorow.AddMonths(1).AddDays(14);
                            var delay = subscriptionStartDate.Subtract(DateTime.UtcNow);

                            _logger.LogInformation($"Adding a subscription starting at {subscriptionStartDate} till {subscriptionEndDate}");

                            var product = await _subscriptionPlanService.FetchSubscriptionPlan();
                            var prices = await _stripeProductsService.GetStripePrices(product.Id);

                            var desiredPrice = prices.Where(p => 
                                p.IsActive 
                                && p.Currency.Equals(checkoutSession.Currency, StringComparison.InvariantCultureIgnoreCase) 
                                && p.UnitAmount == checkoutSession.AmountTotal).FirstOrDefault();

                            var jobIdentifier = _backgroundJobClient.Schedule(() => _checkoutService.CreateDeferredSubscription(checkoutSession.CustomerId, desiredPrice.Id), delay);

                            var res = await _userRepo.ChangeSubscriptionDatesUTC(checkoutSession.CustomerId, subscriptionStartDate, subscriptionEndDate);

                            if (res) _logger.LogInformation($"Subscription dates range for a Stripe Customer id: {checkoutSession.CustomerId} updated to {subscriptionStartDate} - {subscriptionEndDate}");
                            else return BadRequest($"Database result of SubscriptionEndDateUTC update was false for customer with id: {checkoutSession.CustomerId}");
                        }
                        _logger.LogInformation($"Session completed for a subscription with mode of: {checkoutSession.Mode}");
                        return Ok($"Session completed for a subscription with mode of: {checkoutSession.Mode}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ex.Message);
                    }
                }
                else if (stripeEvent.Type == Events.ChargeRefunded)
                {
                    // HACK task 178: this event will be used ONLY in the situation where user has ordered a subscription (with regards to the EU's 14 days cooling off period)
                    // And later on - he declined from using it.
                    // In that case - set both SubscriptionEndDates to DateTime.MinValue
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
