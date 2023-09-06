namespace CreativeCookies.StripeEvents.Contracts
{
    public interface ITargetUrlService
    {
        /// <summary>
        /// Method queries underlying DAL for a record with matching adminEmail, and returns it's targetUrl value.
        /// </summary>
        /// <param name="adminEmail">Key to look up for</param>
        /// <returns>Target URL of that record</returns>
        public string GetDestinationUrl(string adminEmail);
    }
}