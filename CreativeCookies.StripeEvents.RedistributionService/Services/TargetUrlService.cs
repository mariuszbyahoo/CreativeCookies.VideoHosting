using Azure.Data.Tables;
using CreativeCookies.StripeEvents.RedistributionService.Contracts;
using CreativeCookies.StripeEvents.RedistributionService.DAO;
using System.Xml;

namespace CreativeCookies.StripeEvents.RedistributionService.Services
{
    public class TargetUrlService : ITargetUrlService
    {
        public async Task<string> GetDestinationUrlByEmail(string adminEmail, string tableStorageAccountKey)
        {
            var tableClient = new TableClient(new Uri($"https://cccentralstorageaccount.table.core.windows.net/"), "DeployedInstances", new TableSharedKeyCredential("cccentralstorageaccount", tableStorageAccountKey));

            string filter = TableClient.CreateQueryFilter<DeployedInstancesEntity>(e => e.RowKey.Equals(adminEmail));
            await foreach (var entity in tableClient.QueryAsync<DeployedInstancesEntity>(filter))
            {
                return entity.PartitionKey;                
            }
            return string.Empty;
        }
    }
}
