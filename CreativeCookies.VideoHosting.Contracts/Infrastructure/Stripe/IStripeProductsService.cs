using CreativeCookies.VideoHosting.DTOs.Stripe;
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
        Task<SubscriptionPlanDto> UpsertStripeProduct(string productName, string productDescription);

        /// <summary>
        /// Creates a Stripe Price object
        /// </summary>
        /// <param name="productId">Id of a product to which this price is assigned</param>
        /// <param name="currencyCode">Currency code, for an insight, see: <see href="https://stripe.com/docs/currencies">Stripe supported currencies</see></param>
        /// <param name="unitAmount">Smallest currency's amount, for example cents</param>
        /// <returns>Price's Id</returns>
        Task<PriceDto> CreateStripePrice(string productId, string currencyCode, int unitAmount);
        /// <summary>
        /// Retrieves a product's DTO from the database which contains list of price DTOs
        /// </summary>
        /// <param name="productId">id of product to retrieve</param>
        /// <returns>ProductDto</returns>
        SubscriptionPlanDto GetStripeProduct(string productId);

        /// <summary>
        /// Retrieves list of available Prices assigned to the product with supplied Id
        /// </summary>
        /// <param name="productId">Id of product to which prices are assigned</param>
        /// <returns>IList of PriceDto</returns>
        IList<PriceDto> GetStripePrices(string productId);

        /// <summary>
        /// Deletes a Stripe Product from Stripe's infrastructure.
        /// </summary>
        /// <param name="productId">StripeProductId to delete</param>
        /// <returns>void</returns>
        Task DeleteStripeProduct(string productId);
    }
}
