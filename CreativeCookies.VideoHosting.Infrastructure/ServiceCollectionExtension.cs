using Azure.Storage.Blobs;
using Azure.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;

namespace CreativeCookies.VideoHosting.Infrastructure
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddInfrastructureConfiguration(this IServiceCollection services, string keyVaultUrl,
            string storageAccountName, string storageAccountKey, string blobServiceUrl)
        {
            var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
            var stripeSecretKey = client.GetSecret("StripeSecretAPIKey");

            services.AddSingleton(stripeSecretKey.Value.Value);
            services.AddSingleton(x => new StorageSharedKeyCredential(storageAccountName, storageAccountKey));
            services.AddSingleton(x => new BlobServiceClient(new Uri(blobServiceUrl), x.GetRequiredService<StorageSharedKeyCredential>()));
            return services;
        }
    }
}
