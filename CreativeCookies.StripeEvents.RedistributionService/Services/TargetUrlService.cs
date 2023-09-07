using Azure.Data.Tables;
using CreativeCookies.StripeEvents.RedistributionService.Contracts;
using CreativeCookies.StripeEvents.RedistributionService.DAO;
using System.Xml;

namespace CreativeCookies.StripeEvents.RedistributionService.Services
{
    public class TargetUrlService : ITargetUrlService
    {
        public async Task<string> GetDestinationUrl(string adminEmail)
        {
            // Initialize your TableClient
            string accountKey = "CH1ZTJ0J4qBLpURUOMM0wIrRZX99HOBKeSdmYvCeKn45BXtdmjDGQKSU0ZYzjoFZtVkB5ZBoOdnT+AStIOuVTQ==";
            var tableClient = new TableClient(new Uri($"https://cccentralstorageaccount.table.core.windows.net/"), "DeployedInstances", new TableSharedKeyCredential("cccentralstorageaccount", accountKey));

            // HACK TODO: Get StorageConnectionString from App's settings - Azure Web Application's Configuration Key Value Pairs.

            // Construct the filter query
            string filter = TableClient.CreateQueryFilter<DeployedInstancesEntity>(e => e.RowKey.Equals(adminEmail));
            await foreach (var entity in tableClient.QueryAsync<DeployedInstancesEntity>(filter))
            {
                // Access the apiUrl from the entity
                string apiUrl = $"https://{entity.PartitionKey}";
                return apiUrl;
                
            }
            return string.Empty;
        }
    }
}
