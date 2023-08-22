using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe
{
    public  interface IStripeProductsService
    {
        /// <summary>
        /// Creates a new Stripe Product object
        /// </summary>
        /// <param name="productName">Name of the product</param>
        /// <param name="productDescription">Description of this product</param>
        /// <returns>Product's Id</returns>
        Task<string> CreateStripeProductAsync(string productName, string productDescription);

        /// <summary>
        /// Creates a Stripe Price object
        /// </summary>
        /// <param name="productId">Id of a product to which this price is assigned</param>
        /// <param name="currencyCode">Currency code, for an insight, see: <see href="https://stripe.com/docs/currencies">Stripe supported currencies</see></param>
        /// <param name="unitAmount">Smallest currency's amount, for example cents</param>
        /// <returns>Price's Id</returns>
        Task<string> CreateStripePriceAsync(string productId, string currencyCode, int unitAmount);
    }
}
