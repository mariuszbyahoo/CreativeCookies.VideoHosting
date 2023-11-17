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
using CreativeCookies.VideoHosting.Infrastructure.Azure.Wrappers;
using CreativeCookies.VideoHosting.Infrastructure.Azure;
using CreativeCookies.VideoHosting.Infrastructure.Stripe;
using CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe;
using System.Globalization;
using PdfSharp.Fonts;

namespace CreativeCookies.VideoHosting.Infrastructure.ServiceCollectionExtension
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddInfrastructureConfiguration(this IServiceCollection services, string keyVaultUrl,
            string storageAccountName, string storageAccountKey, string blobServiceUrl)
        {
            var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
            KeyVaultSecret stripeSecretKey;
            KeyVaultSecret stripeWebhookSigningKey;
            KeyVaultSecret nettApplicationFee;
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                stripeSecretKey = client.GetSecret("StripeSecretAPIKey");
                stripeWebhookSigningKey = client.GetSecret("StripeSecretWebhookSigningKey");
            }
            else
            {
                stripeSecretKey = client.GetSecret("StripeTestAPIKey");
                stripeWebhookSigningKey = client.GetSecret("StripeTestWebhookSigningKey");
            }
            nettApplicationFee = client.GetSecret("NettApplicationFee");
            services.AddSingleton(w => new ApplicationFeeWrapper(decimal.Parse(nettApplicationFee.Value, CultureInfo.InvariantCulture)));
            services.AddSingleton(w => new StripeSecretKeyWrapper(stripeSecretKey.Value));
            services.AddSingleton(w => new StripeWebhookSigningKeyWrapper(stripeWebhookSigningKey.Value));
            services.AddSingleton(x => new StorageSharedKeyCredential(storageAccountName, storageAccountKey));
            services.AddSingleton(x => new BlobServiceClient(new Uri(blobServiceUrl), x.GetRequiredService<StorageSharedKeyCredential>()));
            GlobalFontSettings.FontResolver = new FileFontResolver();
            return services;
        }
    }
}
