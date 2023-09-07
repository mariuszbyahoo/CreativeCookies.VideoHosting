namespace CreativeCookies.StripeEvents.RedistributionService.Contracts
{
    public interface ITargetUrlService
    {
        public Task<string> GetDestinationUrl(string adminEmail);
    }
}
