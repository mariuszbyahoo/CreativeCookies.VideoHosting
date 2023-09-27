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
        private readonly IConnectAccountsService _connectAccountsService;
        private readonly ISubscriptionPlanService _subscriptionPlanService;

        public StripeProductsService(StripeSecretKeyWrapper stripeKeyWrapper, IConnectAccountsService connectAccountsService, ISubscriptionPlanService subscriptionPlanService)
        {
            _stripeSecretAPIKey = stripeKeyWrapper.Value;
            _connectAccountsService = connectAccountsService;
            _subscriptionPlanService = subscriptionPlanService;
        }

        public async Task<SubscriptionPlanDto> UpsertStripeProduct(string productName, string productDescription)
        {
            StripeConfiguration.ApiKey = _stripeSecretAPIKey;

            SubscriptionPlanDto dto = null;
            var productService = new ProductService();

            if (await _subscriptionPlanService.HasAnyProduct())
            {
                dto = await _subscriptionPlanService.FetchSubscriptionPlan();
                var requestOptions = await GetRequestOptions();

                var updateOptions = new ProductUpdateOptions
                {
                    Active = true,
                    Name = productName,
                    Description = productDescription
                };
                var product = await productService.UpdateAsync(dto.Id, updateOptions, requestOptions);
                dto = new SubscriptionPlanDto(product.Id, product.Name, product.Description);
            }
            else
            {
                var productOptions = new ProductCreateOptions
                {
                    Name = productName,
                    Type = "service",
                    Description = productDescription
                };
                var requestOptions = await GetRequestOptions();
                var product = productService.Create(productOptions, requestOptions);
                dto = new SubscriptionPlanDto(product.Id, product.Name, product.Description);
            }
            return await _subscriptionPlanService.UpsertSubscriptionPlan(dto);
        }

        public async Task DeleteStripeProduct(string productId)
        {
            StripeConfiguration.ApiKey = _stripeSecretAPIKey;

            var productService = new ProductService();
            await productService.DeleteAsync(productId);
        }
        public async Task<SubscriptionPlanDto> GetStripeProduct(string productId)
        {
            StripeConfiguration.ApiKey = _stripeSecretAPIKey;

            var productService = new ProductService();
            var product = productService.Get(productId);

            var result = new SubscriptionPlanDto(product.Id, product.Name, product.Description);
            result.Prices = await GetStripePricesPrivate(product.Id);

            _subscriptionPlanService.UpsertSubscriptionPlan(result);
            return result;
        }

        public async Task<IList<PriceDto>> GetStripePrices(string productId)
        {
            StripeConfiguration.ApiKey = _stripeSecretAPIKey;
            var res = await GetStripePricesPrivate(productId);
            return res;
        }
        public async Task<PriceDto> CreateStripePrice(string productId, string currencyCode, int unitAmount)
        {
            StripeConfiguration.ApiKey = _stripeSecretAPIKey;

            var requestOptions = await GetRequestOptions();
            var priceService = new PriceService();
            var priceOptions = new PriceCreateOptions
            {
                Active = true,
                UnitAmount = unitAmount,
                Currency = currencyCode,
                Product = productId,
                Recurring = new PriceRecurringOptions
                {
                    Interval = "month"
                },
                Nickname = $"Price created at UTC: {DateTime.UtcNow}"
            };

            var price = priceService.Create(priceOptions, requestOptions);
            var res = new PriceDto(price.Id, price.Active, price.ProductId, price.Currency, price.UnitAmount, price.Recurring?.Interval ?? string.Empty);
            return res;
        }

        public async Task<PriceDto> TogglePriceState(string priceId)
        {
            StripeConfiguration.ApiKey = _stripeSecretAPIKey;

            var requestOptions = await GetRequestOptions();
            var priceService = new PriceService();

            var currentPrice = await priceService.GetAsync(priceId, requestOptions: requestOptions);
            var priceOptions = new PriceUpdateOptions()
            {
                Active = currentPrice.Active ? false : true
            };
            var price = await priceService.UpdateAsync(priceId, priceOptions, requestOptions);
            var res = new PriceDto(price.Id, price.Active,price.ProductId, price.Currency, price.UnitAmount, price.Recurring?.Interval ?? string.Empty);
            return res;
        }

        public async Task<PriceDto> ActivateStripePrice(string priceId)
        {
            StripeConfiguration.ApiKey = _stripeSecretAPIKey;

            var requestOptions = await GetRequestOptions();
            var priceService = new PriceService();
            var priceOptions = new PriceUpdateOptions()
            {
                Active = true
            };
            var price = priceService.Update(priceId, priceOptions, requestOptions);
            var res = new PriceDto(price.Id, price.Active, price.ProductId, price.Currency, price.UnitAmount, price.Recurring?.Interval ?? string.Empty);
            return res;
        }

        public async Task<PriceDto> GetPriceById(string priceId)
        {
            StripeConfiguration.ApiKey = _stripeSecretAPIKey;

            var requestOptions = await GetRequestOptions();
            var priceService = new PriceService();
            var res = await priceService.GetAsync(priceId, requestOptions: requestOptions);
            return new PriceDto(res.Id, res.Active, res.ProductId, res.Currency, res.UnitAmount, res.Recurring?.Interval ?? string.Empty);
        }

        #region private

        private async Task<IList<PriceDto>> GetStripePricesPrivate(string productId)
        {
            var priceService = new PriceService();
            var requestOptions = await GetRequestOptions();
            var priceListOptions = new PriceListOptions
            {
                Product = productId
            };

            var prices = await priceService.ListAsync(priceListOptions, requestOptions);

            var result = new List<PriceDto>();
            for (int i = 0; i < prices.Data.Count(); i++)
            {
                result.Add(
                    new PriceDto(
                        prices.Data[i].Id,
                        prices.Data[i].Active,
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

            accountId = _connectAccountsService.GetConnectedAccountId();
            var requestOptions = new RequestOptions();
            requestOptions.StripeAccount = accountId;
            if (string.IsNullOrWhiteSpace(accountId)) return null;
            return requestOptions;
        }

        #endregion
    }
}
