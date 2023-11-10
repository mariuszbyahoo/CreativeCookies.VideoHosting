using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CreativeCookies.VideoHosting.Contracts.Azure;
using CreativeCookies.VideoHosting.Contracts.Infrastructure.Azure;

namespace CreativeCookies.VideoHosting.Infrastructure.Azure
{
    public class MyHubBlobService : IMyHubBlobService
    {
        private readonly IBlobServiceClientWrapper _blobServiceClient;

        public MyHubBlobService(IBlobServiceClientWrapper wrapper)
        {
            _blobServiceClient = wrapper;
        }

        public async Task<bool> DeleteBlob(string blobName, string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            var result = await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
            return result.Value;
        }

        public async Task<BlobContentInfo> UploadPdfToAzureAsync(byte[] pdfContent, string fileName)
        {
            string containerName = "pdf-invoices";
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();

            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            using var memoryStream = new MemoryStream(pdfContent);
            var res = await blobClient.UploadAsync(memoryStream, overwrite: true);
            return res;
        }
    }
}
