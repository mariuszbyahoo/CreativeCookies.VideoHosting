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
        private readonly IStripeProductsService _stripeProductsService;
        private readonly IUsersRepository _usersRepo;
        private readonly RequestOptions _requestOptions;
        private readonly string _clientUrl;
        private readonly string _connectAccountId;

        public CheckoutService(StripeSecretKeyWrapper wrapper, ILogger<CheckoutService> logger, 
            IConnectAccountsRepository connectAccountRepo, IConfiguration configuration, IStripeProductsService stripeProductsService,
            IUsersRepository usersRepo)
        {
            _connectAccountsRepo = connectAccountRepo;
            _stripeApiSecretKey = wrapper.Value;
            _logger = logger;
            _configuration = configuration;
            _stripeProductsService = stripeProductsService;
            _usersRepo = usersRepo; 
            _clientUrl = _configuration.GetValue<string>("ClientUrl");
            _connectAccountId = _connectAccountsRepo.GetConnectedAccountId();
            _requestOptions = new RequestOptions { StripeAccount = _connectAccountId };
        }

        public async Task<string> CreateNewSession(string priceId, string stripeCustomerId, bool HasDeclinedCoolingOffPeriod = false)
        {
            Session session;
            StripeConfiguration.ApiKey = _stripeApiSecretKey;

            if (string.IsNullOrWhiteSpace(_connectAccountId))
            {
                _logger.LogError("No connect account found in database, aborting creation of new session");
                return string.Empty;
            }

            var service = new SessionService();

            if (HasDeclinedCoolingOffPeriod)
            {
                var successUrl = $"{_clientUrl}/success?sessionId="; 
                                                                     
                successUrl += "{CHECKOUT_SESSION_ID}";
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
                    SuccessUrl = successUrl,
                    CancelUrl = $"{_clientUrl}/cancel",
                };
                session = service.Create(options, _requestOptions);

                return session.Url;
            }
            else
            {
                var successUrl = $"{_clientUrl}/ordersuccess?sessionId="; 
                                                                     
                successUrl += "{CHECKOUT_SESSION_ID}";
                var price = await _stripeProductsService.GetPriceById(priceId);
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
                                UnitAmount = price.UnitAmount,
                                Currency = price.Currency,
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
                        ApplicationFeeAmount = long.Parse($"{price.UnitAmount * 0.1}"),
                        SetupFutureUsage = "off_session"
                    },
                    SuccessUrl = successUrl,
                    CancelUrl = $"{_clientUrl}/cancel"

                };
                var sessionService = new SessionService();
                session = sessionService.Create(sessionOptions, _requestOptions);

                return session.Url;
            }
        }

        public async Task<bool> IsSessionPaymentPaid(string sessionId)
        {
            StripeConfiguration.ApiKey = _stripeApiSecretKey;

            var service = new SessionService();
            Session session = await service.GetAsync(sessionId, requestOptions: _requestOptions);

            return session.PaymentStatus.Equals("paid");
        }

        public string CreateDeferredSubscription(string customerId, string priceId)
        {
            StripeConfiguration.ApiKey = _stripeApiSecretKey;
            var beginningOfTommorow = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 0, 0, 0, DateTimeKind.Utc).AddDays(1);

            var options = new SubscriptionCreateOptions
            {
                Customer = customerId,
                Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions
                    {
                        Price = priceId
                    },
                },
                TrialEnd = beginningOfTommorow.AddDays(14)
                // Set TrialEnd to next day's beginning and add 14 days for the trial period
            };

            var service = new SubscriptionService();
            Subscription subscription = service.Create(options, requestOptions: _requestOptions);
            return subscription.Id;
        }

        public async Task<bool> RefundCanceledOrder(string userId)
        {
            var res = false;
            var user = await _usersRepo.GetUserById(userId);
            StripeConfiguration.ApiKey = _stripeApiSecretKey;

            try
            {
                // Retrieve the customer's latest payment intent
                var paymentIntentService = new PaymentIntentService();
                var paymentIntentListOptions = new PaymentIntentListOptions
                {
                    Customer = user.StripeCustomerId,
                    Limit = 1
                };
                var paymentIntents = paymentIntentService.List(paymentIntentListOptions, _requestOptions);
                var latestPaymentIntent = paymentIntents.FirstOrDefault();

                if (latestPaymentIntent != null)
                {
                    // Initiate a refund for the payment intent
                    var refundService = new RefundService();
                    var refundOptions = new RefundCreateOptions
                    {
                        PaymentIntent = latestPaymentIntent.Id
                    };
                    var refund = refundService.Create(refundOptions, _requestOptions);

                    if (refund.Status.Equals("succeeded", StringComparison.InvariantCultureIgnoreCase))
                    {
                        res = true;
                    }
                }
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, $"StripeException occured: {ex.Message}, {ex.StripeError}, {ex.HelpLink}, Status Code: {ex.HttpStatusCode}, {ex.StackTrace}, InnerException: {ex.InnerException}");
            }

            return res;
        }
    }
}
