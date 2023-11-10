using Azure.Core;
using Azure.Messaging.ServiceBus;
using CreativeCookies.StripeEvents.DTOs;
using CreativeCookies.VideoHosting.Contracts.Email;
using CreativeCookies.VideoHosting.Contracts.Infrastructure;
using CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.Services.Stripe;
using CreativeCookies.VideoHosting.DTOs;
using CreativeCookies.VideoHosting.DTOs.OAuth;
using CreativeCookies.VideoHosting.Infrastructure.Azure.Wrappers;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Infrastructure.Stripe
{
    public class StripeMessageReceiver : IHostedService
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ServiceBusProcessor _processor;
        private readonly StripeWebhookSigningKeyWrapper _wrapper;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger _logger;
        private readonly string _connectAccountId;


        public StripeMessageReceiver(IBackgroundJobClient backgroundJobClient, StripeWebhookSigningKeyWrapper wrapper,
             IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, ILogger<StripeMessageReceiver> logger)
        {
            _logger = logger;
            _serviceBusClient = new ServiceBusClient(configuration.GetValue<string>("ServiceBusConnectionString"));
            _wrapper = wrapper;
            _serviceScopeFactory = serviceScopeFactory;
            _backgroundJobClient = backgroundJobClient;
            _processor = _serviceBusClient.CreateProcessor("stripe_events_queue", new ServiceBusProcessorOptions());
            _processor.ProcessMessageAsync += MessageHandler;
            _processor.ProcessErrorAsync += ErrorHandler;
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var connectAccountsService = scope.ServiceProvider.GetRequiredService<IConnectAccountsService>();
                _connectAccountId = connectAccountsService.GetConnectedAccountId();
            }
        }

        public async Task StartAsync(CancellationToken token)
        {
            await _processor.StartProcessingAsync();
        }

        public async Task StopAsync(CancellationToken token)
        {
            await _processor.StopProcessingAsync();
            await _serviceBusClient.DisposeAsync();
        }

        public async Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError($"An unexpected exception occured inside of a ServiceBusReceiver, {args.Exception.Message}, {args.Exception.Source}, {args.Exception.StackTrace}, {args.Exception.InnerException}, {args.Exception.Data}");
        }

        public async Task MessageHandler(ProcessMessageEventArgs args)
        {
            var stripeEventDto = System.Text.Json.JsonSerializer.Deserialize<StripeEventDTO>(args.Message.Body.ToString());

            // HACK: check, is this event intended for the account used by this instance
            // if not - then log information about received event and return.
            // if so - then perform

            _logger.LogInformation("StripeMessageReceiver called");
            string endpointSecret = _wrapper.Value;

            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var userRepo = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
                    var subscriptonPlanService = scope.ServiceProvider.GetRequiredService<ISubscriptionPlanService>();

                    var stripeEvent = EventUtility.ConstructEvent(stripeEventDto.JsonRequestBody,
                    stripeEventDto.StripeSignature,
                    endpointSecret);
                    _logger.LogInformation($"Checking is this instance's accountId: '{_connectAccountId}' equal to the stripe event's accountId: {stripeEvent.Account}");
                    if (stripeEvent.Account.Equals(_connectAccountId))
                    {

                        if (stripeEvent.Type == Events.ProductCreated || stripeEvent.Type == Events.ProductUpdated)
                        {
                            _logger.LogInformation($"StripeMessageReceiver with event type of {stripeEvent.Type}");
                            var product = stripeEvent.Data.Object as Product;
                            if (product != null)
                            {
                                await subscriptonPlanService.UpsertSubscriptionPlan(new DTOs.Stripe.SubscriptionPlanDto(product.Id, product.Name, product.Description));
                                _logger.LogInformation($"StripeMessageReceiver product upserted: {product.ToJson()}");
                            }
                        }
                        else if (stripeEvent.Type == Events.ProductDeleted)
                        {
                            _logger.LogInformation($"StripeMessageReceiver with event type of {stripeEvent.Type}");
                            var product = stripeEvent.Data.Object as Product;
                            if (product != null)
                            {
                                await subscriptonPlanService.DeleteSubscriptionPlan(product.Id);
                                _logger.LogInformation($"StripeMessageReceiver product deleted: {product.ToJson()}");
                            }
                        }
                        else if (stripeEvent.Type == Events.AccountUpdated)
                        {
                            _logger.LogInformation($"StripeMessageReceiver with event type of {stripeEvent.Type}");
                            var account = stripeEvent.Data.Object as Account;
                            var connectAccountsSrv = scope.ServiceProvider.GetRequiredService<IConnectAccountsService>();
                            await connectAccountsSrv.EnsureSaved(account.Id);
                            _logger.LogInformation($"StripeMessageReceiver account updated: {account.ToJson()}");
                        }
                        else if (stripeEvent.Type == Events.InvoicePaymentSucceeded)
                        {
                            _logger.LogInformation($"StripeMessageReceiver with event type of {stripeEvent.Type}");
                            var invoice = stripeEvent.Data.Object as Invoice;
                            var invoicePeriodEnd = invoice.Lines.Data[0].Period.End;
                            var accessPeriodEnd = new DateTime(invoicePeriodEnd.Year, invoicePeriodEnd.Month, invoicePeriodEnd.Day, 0, 0, 0, DateTimeKind.Utc).AddDays(1);
                            var res = userRepo.ChangeSubscriptionDatesUTC(invoice.CustomerId, invoice.Lines.Data[0].Period.Start, accessPeriodEnd, false);
                            if (res) _logger.LogInformation($"Subscription dates range for a Stripe Customer id: {invoice.CustomerId} updated to {invoice.Lines.Data[0].Period.Start} - {accessPeriodEnd}");
                            else _logger.LogError($"Database result of SubscriptionEndDateUTC update was false for customer with id: {invoice.CustomerId}");

                            await ProcessInvoice(invoice.Customer.Id, invoice.AmountPaid, invoice.Currency);
                        }
                        else if (stripeEvent.Type == Events.CheckoutSessionCompleted)
                        {
                            try
                            {
                                _logger.LogInformation($"StripeMessageReceiver with event type of {stripeEvent.Type}");
                                var checkoutSession = stripeEvent.Data.Object as Session;

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

                                    if (customer.StripeResponse.StatusCode == System.Net.HttpStatusCode.OK)
                                        _logger.LogInformation($"Default payment method set for customer {checkoutSession.CustomerId}");
                                    else
                                        _logger.LogError($"Setting payment as default has not respond with 200 due to error {customer.StripeResponse.StatusCode}, requestId: {customer.StripeResponse.RequestId} with paymentIntent data. An ID: {paymentIntent.Id}, methodId: {paymentIntent.PaymentMethodId}, and sourceId: {paymentIntent.SourceId}");

                                    // HACK: Adjust to subscription start on 00:00 UTC of the next day.
                                    var beginningOfTommorow = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 0, 0, 0, DateTimeKind.Utc).AddDays(1);
                                    var subscriptionStartDate = beginningOfTommorow.AddDays(14);
                                    var subscriptionEndDate = beginningOfTommorow.AddMonths(1).AddDays(14);
                                    var delay = subscriptionStartDate.Subtract(DateTime.UtcNow);

                                    _logger.LogInformation($"Adding a subscription starting at {subscriptionStartDate} till {subscriptionEndDate}");
                                    var stripeProductsService = scope.ServiceProvider.GetRequiredService<IStripeProductsService>();

                                    var product = await subscriptonPlanService.FetchSubscriptionPlan();
                                    var prices = await stripeProductsService.GetStripePrices(product.Id);

                                    var desiredPrice = prices.Where(p =>
                                        p.IsActive
                                        && p.Currency.Equals(checkoutSession.Currency, StringComparison.InvariantCultureIgnoreCase)
                                        && p.UnitAmount == checkoutSession.AmountTotal).FirstOrDefault();

                                    var checkoutService = scope.ServiceProvider.GetRequiredService<ICheckoutService>();

                                    var jobIdentifier = _backgroundJobClient.Schedule(() => checkoutService.CreateDeferredSubscription(checkoutSession.CustomerId, desiredPrice.Id), delay);
                                    if (!string.IsNullOrWhiteSpace(jobIdentifier)) userRepo.AssignHangfireJobIdToUser(checkoutSession.CustomerId, jobIdentifier);

                                    var res = userRepo.ChangeSubscriptionDatesUTC(checkoutSession.CustomerId, subscriptionStartDate, subscriptionEndDate);

                                    if (res) _logger.LogInformation($"Subscription dates range for a Stripe Customer id: {checkoutSession.CustomerId} updated to {subscriptionStartDate} - {subscriptionEndDate}");
                                    else _logger.LogError($"Database result of SubscriptionEndDateUTC update was false for customer with id: {checkoutSession.CustomerId}");

                                    await ProcessInvoice(checkoutSession.CustomerId, paymentIntent.AmountReceived, paymentIntent.Currency);
                                }
                                _logger.LogInformation($"Session completed for a subscription with mode of: {checkoutSession.Mode}");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, ex.Message);
                            }
                        }
                        else if (stripeEvent.Type == Events.ChargeRefunded)
                        {
                            _logger.LogInformation($"StripeMessageReceiver with event type of {stripeEvent.Type}");
                            var accountId = stripeEvent.Account;
                            var charge = stripeEvent.Data.Object as Charge;
                            var res = userRepo.ChangeSubscriptionDatesUTC(charge.CustomerId, DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)), DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)), false);
                            if (res) _logger.LogInformation($"SubscriptionEndDateUTC of Stripe Customer id: {charge.CustomerId} updated to {DateTime.UtcNow}");
                            else _logger.LogError($"Database result of SubscriptionEndDateUTC update was false for customer with id: {charge.CustomerId}");
                        }
                        else
                        {
                            _logger.LogWarning($"Unexpected Stripe event's type: {stripeEvent.ToJson()}");
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"Received an event intended for another Stripe account: {stripeEvent.Account}");
                    }
                }
            }
            catch (StripeException e)
            {
                _logger.LogError(e, e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message, e.StackTrace);
            }

            _logger.LogInformation("StripeMessageReceiver returns 200");

            await args.CompleteMessageAsync(args.Message);
        }

        private async Task ProcessInvoice(string stripeCustomerId, decimal amount, string currency)
        {
            MyHubUserDto user;
            MerchantDto merchant;
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var usersRepo = scope.ServiceProvider.GetRequiredService<IUsersRepository>();
                var merchantRepository = scope.ServiceProvider.GetRequiredService<IMerchantRepository>();
                user = await usersRepo.GetUserByStripeCustomerId(stripeCustomerId);
                merchant = await merchantRepository.GetMerchant();
            }

            if (user?.Address == null)
                _logger.LogCritical($"No address for user: {user?.UserEmail} found, aborting invoice generation");
            if (merchant == null)
                _logger.LogCritical($"No merchant found, aborting invoice generation");

            var canGenerateInvoice = merchant != null && user.Address != null;
            if (canGenerateInvoice)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var invoiceService = scope.ServiceProvider.GetRequiredService<IInvoiceService>();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    var invoiceData = await invoiceService.GenerateInvoicePdf(amount, currency, user.Address, merchant);

                    await emailService.SendInvoiceAsync(user.UserEmail, invoiceData.InvoiceNumber, "TODOWebsiteName", invoiceData);
                }
            }
        }
    }
}
