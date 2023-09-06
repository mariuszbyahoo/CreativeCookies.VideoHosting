using CreativeCookies.StripeEvents.Contracts;

namespace CreativeCookies.StripeEvents.Services
{
    public class TargetUrlService : ITargetUrlService
    {
        public string GetDestinationUrl(string adminEmail)
        {
            // 1. Get record from underlying DAL with matching adminEmail
            // 2. Return it's destinationUrl value.
            throw new NotImplementedException();
        }
    }
}