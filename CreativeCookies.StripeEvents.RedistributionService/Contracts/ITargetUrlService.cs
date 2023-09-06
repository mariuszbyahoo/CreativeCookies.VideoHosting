namespace CreativeCookies.StripeEvents.RedistributionService.Contracts
{
    public interface ITargetUrlService
    {
        public string GetDestinationUrl(string adminUrl);
    }
}
