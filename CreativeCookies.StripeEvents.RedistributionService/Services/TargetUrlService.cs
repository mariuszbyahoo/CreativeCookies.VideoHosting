using CreativeCookies.StripeEvents.RedistributionService.Contracts;

namespace CreativeCookies.StripeEvents.RedistributionService.Services
{
    public class TargetUrlService : ITargetUrlService
    {
        public string GetDestinationUrl(string adminUrl)
        {
            // 1. Get record from underlying DAL with matching adminEmail
            // 2. Return it's destinationUrl value.
            throw new NotImplementedException();
        }
    }
}
