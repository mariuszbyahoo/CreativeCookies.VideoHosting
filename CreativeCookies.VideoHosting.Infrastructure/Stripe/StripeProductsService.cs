using CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe;
using CreativeCookies.VideoHosting.Infrastructure.Azure.Wrappers;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Infrastructure.Stripe
{
    public class StripeProductsService : IStripeProductsService
    {
        private readonly string _stripeSecretAPIKey;
        public StripeProductsService(StripeSecretKeyWrapper stripeKeyWrapper)
        {
            _stripeSecretAPIKey = stripeKeyWrapper.Value;
            StripeConfiguration.ApiKey = _stripeSecretAPIKey;
        }

        public string CreateStripePrice(string productId, string currencyCode, int unitAmount)
        {
            throw new NotImplementedException();
        }

        public string CreateStripeProduct(string productName, string productDescription)
        {
            var productService = new ProductService();
            var productOptions = new ProductCreateOptions
            {
                Name = productName,
                Type = "service",
                Description = productDescription
            };

            var product = productService.Create(productOptions);
            return product.Id;
        }
    }
}
