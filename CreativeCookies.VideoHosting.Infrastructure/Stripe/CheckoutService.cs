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
        private readonly string _clientUrl;
        private readonly string _connectAccountId;

        public CheckoutService(StripeSecretKeyWrapper wrapper, ILogger<CheckoutService> logger, 
            IConnectAccountsRepository connectAccountRepo, IConfiguration configuration, IStripeProductsService stripeProductsService)
        {
            _connectAccountsRepo = connectAccountRepo;
            _stripeApiSecretKey = wrapper.Value;
            _logger = logger;
            _configuration = configuration;
            _stripeProductsService = stripeProductsService;
            _clientUrl = _configuration.GetValue<string>("ClientUrl");
            _connectAccountId = _connectAccountsRepo.GetConnectedAccountId();
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

            var requestOptions = new RequestOptions
            {
                StripeAccount = _connectAccountId,
            };
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
                session = service.Create(options, requestOptions);

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
                        ApplicationFeeAmount = long.Parse($"{price.UnitAmount * 0.1}")
                    },
                    SuccessUrl = successUrl,
                    CancelUrl = $"{_clientUrl}/cancel",
                };
                var sessionService = new SessionService();
                session = sessionService.Create(sessionOptions, requestOptions);

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

        public async Task<string> CreateDeferredSubscription(string customerId, string priceId)
        {
            StripeConfiguration.ApiKey = _stripeApiSecretKey;
            var daysAmount = (DateTime.UtcNow.AddDays(1) - DateTime.UtcNow).Days;
            var requestOptions = new RequestOptions() { StripeAccount = _connectAccountId };

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
                TrialPeriodDays = daysAmount,
                TrialSettings = new SubscriptionTrialSettingsOptions
                {
                    EndBehavior = new SubscriptionTrialSettingsEndBehaviorOptions
                    {
                        MissingPaymentMethod = "cancel"
                    }
                }
            };

            var service = new SubscriptionService();
            Subscription subscription = service.Create(options, requestOptions: requestOptions);
            return subscription.Id;
        }
    }
}
