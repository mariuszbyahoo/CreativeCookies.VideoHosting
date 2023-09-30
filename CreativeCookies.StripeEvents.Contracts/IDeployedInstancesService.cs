using Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.StripeEvents.Contracts
{
    public interface IDeployedInstancesService
    {
        public Task<string> GetDestinationUrlByEmail(string adminEmail, string tableStorageAccountKey);

        public Task<string> GetDestinationUrlByAccountId(string accountId, string tableStorageAccountKey);

        public Task<Response> UpdateAccountId(string adminEmail, string accountId, string tableStorageAccountKey);
    }
}
