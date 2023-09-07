using Azure;
using Azure.Data.Tables;

namespace CreativeCookies.StripeEvents.RedistributionService.DAO
{
    public class DeployedInstancesEntity :ITableEntity
    {
        /// <summary>
        /// Stripe Connect account's ID
        /// </summary>
        public string? StripeConnectId { get; set; }
        /// <summary>
        /// API address without protocol at the beginning and a slash at the end, like: api.myhub.com.pl
        /// </summary>
        public string PartitionKey { get; set; }
        /// <summary>
        /// email address of an admin account
        /// </summary>
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
