﻿using CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe;
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

        public async Task<string> CreateNewSession(string priceId, string stripeCustomerId)
        {
            StripeConfiguration.ApiKey = _stripeApiSecretKey;
            if (string.IsNullOrWhiteSpace(_connectAccountId))
            {
                _logger.LogError("No connect account found in database, aborting creation of new session");
                return string.Empty;
            }
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
                    ApplicationFeePercent = 10,
                },
                BillingAddressCollection = "required",
                SuccessUrl = successUrl, 
                CancelUrl = $"{_clientUrl}/cancel",
            };

            var requestOptions = new RequestOptions
            {
                StripeAccount = _connectAccountId,
            };
            var service = new SessionService();
            Session session = service.Create(options, requestOptions);
            
            return session.Url;
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
