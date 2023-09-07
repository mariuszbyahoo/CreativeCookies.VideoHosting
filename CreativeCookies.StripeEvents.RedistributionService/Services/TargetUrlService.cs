using Azure.Data.Tables;
using CreativeCookies.StripeEvents.RedistributionService.Contracts;
using CreativeCookies.StripeEvents.RedistributionService.DAO;
using System.Xml;

namespace CreativeCookies.StripeEvents.RedistributionService.Services
{
    public class TargetUrlService : ITargetUrlService
    {
        public string GetDestinationUrl(string adminEmail)
        {
            // Initialize your TableClient
            string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=cccentralstorageaccount;AccountKey=CH1ZTJ0J4qBLpURUOMM0wIrRZX99HOBKeSdmYvCeKn45BXtdmjDGQKSU0ZYzjoFZtVkB5ZBoOdnT+AStIOuVTQ==;EndpointSuffix=core.windows.net";
            var tableClient = new TableClient(new Uri($"https://cccentralstorageaccount.table.core.windows.net/"), "DeployedInstances", new TableSharedKeyCredential("<DeployedInstances>", storageConnectionString));

            // HACK TODO: Get StorageConnectionString from App's settings - Azure Web Application's Configuration Key Value Pairs.

            // Construct the filter query
            string filter = TableClient.CreateQueryFilter<DeployedInstancesEntity>(e => e.RowKey.Equals(adminEmail));
            var entity = tableClient.Query<DeployedInstancesEntity>(filter).FirstOrDefault();
            if (entity == null) return string.Empty;
            else return entity.PartitionKey;
        }
    }
}
