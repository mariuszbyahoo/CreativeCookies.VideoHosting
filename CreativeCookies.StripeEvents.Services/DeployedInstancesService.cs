﻿using Azure;
using Azure.Data.Tables;
using CreativeCookies.StripeEvents.Contracts;
using CreativeCookies.StripeEvents.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.StripeEvents.Services
{
    public class DeployedInstancesService : IDeployedInstancesService
    {
        public async Task<string> GetDestinationUrlByAccountId(string accountId, string tableStorageAccountKey)
        {
            var tableClient = new TableClient(new Uri($"https://cccentralstorageaccount.table.core.windows.net/"), "DeployedInstances", new TableSharedKeyCredential("cccentralstorageaccount", tableStorageAccountKey));

            string filter = TableClient.CreateQueryFilter<DeployedInstancesEntity>(e => e.StripeConnectAccountId.Equals(accountId));
            await foreach (var entity in tableClient.QueryAsync<DeployedInstancesEntity>(filter))
            {
                return entity.PartitionKey;
            }
            return string.Empty;
        }

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

        public async Task<Response> UpdateAccountId(string adminEmail, string accountId, string tableStorageAccountKey)
        {
            DeployedInstancesEntity deployedInstance = null;
            var tableClient = new TableClient(new Uri($"https://cccentralstorageaccount.table.core.windows.net/"), "DeployedInstances", new TableSharedKeyCredential("cccentralstorageaccount", tableStorageAccountKey));

            string filter = TableClient.CreateQueryFilter<DeployedInstancesEntity>(e => e.RowKey.Equals(adminEmail));
            await foreach (var entity in tableClient.QueryAsync<DeployedInstancesEntity>(filter))
            {
                deployedInstance = entity;
            }
            deployedInstance.StripeConnectAccountId = accountId;
            var updateResponse = await tableClient.UpdateEntityAsync(deployedInstance, deployedInstance.ETag);
            return updateResponse;
        }


    }

}
