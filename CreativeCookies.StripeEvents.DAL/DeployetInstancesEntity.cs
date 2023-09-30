using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.StripeEvents.DAL
{
    public class DeployedInstancesEntity : ITableEntity
    {
        /// <summary>
        /// Stripe Connect account's ID
        /// </summary>
        public string? StripeConnectAccountId { get; set; }
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
