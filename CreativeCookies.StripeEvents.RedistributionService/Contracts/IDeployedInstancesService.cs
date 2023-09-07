using Azure;

namespace CreativeCookies.StripeEvents.RedistributionService.Contracts
{
    public interface IDeployedInstancesService
    {
        public Task<string> GetDestinationUrlByEmail(string adminEmail, string tableStorageAccountKey);

        public Task<string> GetDestinationUrlByAccountId(string accountId, string tableStorageAccountKey);

        public Task<Response> InsertAccountId(string adminEmail, string accountId, string tableStorageAccountKey);
    }
}
