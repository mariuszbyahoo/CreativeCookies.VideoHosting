using CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe;
using CreativeCookies.VideoHosting.Contracts.Services.Stripe;
using CreativeCookies.VideoHosting.DTOs.Stripe;
using CreativeCookies.VideoHosting.Infrastructure.Azure.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Infrastructure.Stripe
{
    public class StripeProductsService : IStripeProductsService
    {
        private readonly string _stripeSecretAPIKey;
        private readonly IServiceProvider _serviceProvider;

        public StripeProductsService(StripeSecretKeyWrapper stripeKeyWrapper, IServiceProvider serviceProvider)
        {
            _stripeSecretAPIKey = stripeKeyWrapper.Value;
            StripeConfiguration.ApiKey = _stripeSecretAPIKey;
            _serviceProvider = serviceProvider;
        }

        public async Task<PriceDto> CreateStripePrice(string productId, string currencyCode, int unitAmount)
        {
            var requestOptions = await GetRequestOptions();
            var priceService = new PriceService();
            var priceOptions = new PriceCreateOptions
            {
                UnitAmount = unitAmount,
                Currency = currencyCode,
                Product = productId,
                Recurring = new PriceRecurringOptions
                {
                    Interval = "month"
                }
            };

            var price = priceService.Create(priceOptions, requestOptions);
            var res = new PriceDto(price.Id, price.ProductId, price.Currency, price.UnitAmount, price.Recurring?.Interval ?? string.Empty);
            return res;
        }

        public async Task<SubscriptionPlanDto> UpsertStripeProduct(string productName, string productDescription)
        {
            SubscriptionPlanDto res = null;
            var productService = new ProductService();
            var productOptions = new ProductCreateOptions
            {
                Name = productName,
                Type = "service",
                Description = productDescription
            };
            var requestOptions = await GetRequestOptions();
            var product = productService.Create(productOptions, requestOptions);
            var dto = new SubscriptionPlanDto(product.Id, product.Name, product.Description);

            using (var scope = _serviceProvider.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<ISubscriptionPlanService>();
                res = await service.UpsertSubscriptionPlan(dto);
            }
            return res;
        }

        public async Task DeleteStripeProduct(string productId)
        {
            var productService = new ProductService();
            await productService.DeleteAsync(productId);
        }

        public IList<PriceDto> GetStripePrices(string productId)
        {
            return GetStripePricesPrivate(productId);
        }

        public SubscriptionPlanDto GetStripeProduct(string productId)
        {
            var productService = new ProductService();
            var product = productService.Get(productId);

            var result = new SubscriptionPlanDto(product.Id, product.Name, product.Description);
            result.Prices = GetStripePricesPrivate(product.Id);
            
            using (var scope = _serviceProvider.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<ISubscriptionPlanService>();
                service.UpsertSubscriptionPlan(result);
            }
            return result;
        }

        private IList<PriceDto> GetStripePricesPrivate(string productId)
        {
            var priceService = new PriceService();
            var priceListOptions = new PriceListOptions
            {
                Product = productId,
                Limit = 10  
            };

            var prices = priceService.List(priceListOptions);

            var result = new List<PriceDto>();
            for (int i = 0; i < prices.Data.Count(); i++)
            {
                result.Add(
                    new PriceDto(
                        prices.Data[i].Id,
                        prices.Data[i].ProductId,
                        prices.Data[i].Currency,
                        prices.Data[i].UnitAmount,
                        prices.Data[i].Recurring?.Interval ?? string.Empty));
            }
            return result;
        }

        private async Task<RequestOptions?> GetRequestOptions()
        {
            var accountId = string.Empty;
            using (var scope = _serviceProvider.CreateScope())
            {
                var connectAccountsService = _serviceProvider.GetRequiredService<IConnectAccountsService>();
                accountId = await connectAccountsService.GetConnectedAccountId();
            }
            var requestOptions = new RequestOptions();
            requestOptions.StripeAccount = accountId;
            if (string.IsNullOrWhiteSpace(accountId)) return null;
            return requestOptions;
        }

        // HACK: Add editProduct method
    }
}
