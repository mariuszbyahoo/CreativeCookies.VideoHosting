using CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe;
using Stripe.Checkout;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CreativeCookies.VideoHosting.Infrastructure.Azure.Wrappers;
using Microsoft.Extensions.Logging;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace CreativeCookies.VideoHosting.Infrastructure.Stripe
{
    public class CheckoutService : ICheckoutService
    {
        private readonly string _stripeApiSecretKey;
        private readonly ILogger<CheckoutService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IConnectAccountsRepository _connectAccountsRepo;
        private readonly string _clientUrl;
        private readonly string _connectAccountId;

        public CheckoutService(StripeSecretKeyWrapper wrapper, ILogger<CheckoutService> logger, IConnectAccountsRepository connectAccountRepo, IConfiguration configuration)
        {
            _connectAccountsRepo = connectAccountRepo;
            _stripeApiSecretKey = wrapper.Value;
            _logger = logger;
            _configuration = configuration;
            _clientUrl = _configuration.GetValue<string>("ClientUrl");
            _connectAccountId = _connectAccountsRepo.GetConnectedAccountId();
        }

        public async Task<string> CreateNewSession(string priceId, string stripeCustomerId, bool isCoolingOffPeriodApplicable = false)
        {
            // HACK Task 178 :
            // Depending from the new optional argument : bool isCoolingOffPeriodApplicable 
            // 1. create a one-time invoice within the session, and return the session.URL

            Session session;
            StripeConfiguration.ApiKey = _stripeApiSecretKey;
            var successUrl = $"{_clientUrl}/success?sessionId="; // HACK Task 178: design different success page, success expects 
            // user has not choosen to use 14 days cooling off period
            successUrl += "{CHECKOUT_SESSION_ID}";
            

            if (string.IsNullOrWhiteSpace(_connectAccountId))
            {
                _logger.LogError("No connect account found in database, aborting creation of new session");
                return string.Empty;
            }

            var requestOptions = new RequestOptions
            {
                StripeAccount = _connectAccountId,
            };
            var service = new SessionService();

            if (isCoolingOffPeriodApplicable)
            {
                // HACK Task 178 
                // Is it better to use invoice or session.checkout.completed? Maybe it'd be better in both scenarios?

                //var invoiceItemCreateOptions = new InvoiceItemCreateOptions
                //{
                //    Customer = stripeCustomerId,
                //    UnitAmount = long.Parse(amount),
                //    Currency = currency,
                //};
                //var invoiceItemService = new InvoiceItemService();
                //var invoiceItem = invoiceItemService.Create(invoiceItemCreateOptions, requestOptions);

                //var invoiceOptions = new InvoiceCreateOptions
                //{
                //    Customer = stripeCustomerId,
                //    AutoAdvance = true
                //};
                //var invoiceService = new InvoiceService();
                //var invoice = invoiceService.Create(invoiceOptions, requestOptions);

                long amount = 1000; // HACK: take this from the price selected by the end user
                var currency = "PLN"; // HACK: this one too
                var sessionOptions = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string>
                    {
                        "card",
                    },
                    Mode = "payment",
                    Customer = stripeCustomerId,
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = amount,
                                Currency = currency,
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = "Order for subscription with 14 days cooling off period"
                                }
                            },
                            Quantity = 1,
                        }
                    },
                    PaymentIntentData = new SessionPaymentIntentDataOptions
                    {
                        ApplicationFeeAmount = long.Parse($"{amount * 0.1}") 
                    },
                    SuccessUrl = successUrl,
                    CancelUrl = $"{_clientUrl}/cancel",
                };
                var sessionService = new SessionService();
                session = sessionService.Create(sessionOptions, requestOptions);

                return session.Url;
            }
            else
            {
                var options = new SessionCreateOptions
                {
                    Customer = stripeCustomerId,
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            Price = priceId,
                            Quantity = 1,
                        },
                    },
                    Mode = "subscription",
                    SubscriptionData = new SessionSubscriptionDataOptions
                    {
                        ApplicationFeePercent = 10, // HACK: Make this configurable amount of percent
                    },
                    BillingAddressCollection = "required", // HACK: is this necessary?
                    SuccessUrl = successUrl,
                    CancelUrl = $"{_clientUrl}/cancel",
                };
                session = service.Create(options, requestOptions);

                return session.Url;
            }
        }

        public async Task<bool> IsSessionPaymentPaid(string sessionId)
        {
            StripeConfiguration.ApiKey = _stripeApiSecretKey;

            var service = new SessionService();
            var requestOptions = new RequestOptions() { StripeAccount = _connectAccountId };
            Session session = await service.GetAsync(sessionId, requestOptions: requestOptions);

            return session.PaymentStatus.Equals("paid");
        }
    }
}
