namespace CreativeCookies.StripeEvents.RedistributionService.Contracts
{
    public interface ITargetUrlService
    {
        public Task<string> GetDestinationUrlByEmail(string adminEmail, string tableStorageAccountKey);
    }
}
