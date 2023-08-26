using CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe;
using CreativeCookies.VideoHosting.DTOs.Stripe;
using CreativeCookies.VideoHosting.Infrastructure.Azure.Wrappers;
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
        //HACK: Add StripeProductRepo store productId in the database

        public StripeProductsService(StripeSecretKeyWrapper stripeKeyWrapper)
        {
            _stripeSecretAPIKey = stripeKeyWrapper.Value;
            StripeConfiguration.ApiKey = _stripeSecretAPIKey;
        }

        public PriceDto CreateStripePrice(string productId, string currencyCode, int unitAmount)
        {
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

            var price = priceService.Create(priceOptions);
            return new PriceDto(price.Id, price.ProductId, price.Currency, price.UnitAmount, price.Recurring?.Interval ?? string.Empty);
        }

        public ProductDto CreateStripeProduct(string productName, string productDescription)
        {
            var productService = new ProductService();
            var productOptions = new ProductCreateOptions
            {
                Name = productName,
                Type = "service",
                Description = productDescription
            };

            var product = productService.Create(productOptions);
            return new ProductDto(product.Id, product.Name, product.Description);
        }

        public IList<PriceDto> GetStripePrices(string productId)
        {
            return GetStripePricesPrivate(productId);
        }

        public ProductDto GetStripeProduct(string productId)
        {
            var productService = new ProductService();
            var product = productService.Get(productId);

            var result = new ProductDto(product.Id, product.Name, product.Description);
            result.Prices = GetStripePricesPrivate(product.Id);
            return result;
        }

        private IList<PriceDto> GetStripePricesPrivate(string productId)
        {
            var priceService = new PriceService();
            var priceListOptions = new PriceListOptions
            {
                Product = productId,
                Limit = 10  // Limit to 10, you can paginate for more
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

        // HACK: Add editProduct method
    }
}
