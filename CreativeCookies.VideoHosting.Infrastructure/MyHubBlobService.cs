using Azure.Storage.Blobs.Models;
using CreativeCookies.VideoHosting.Contracts.Azure;
using CreativeCookies.VideoHosting.Contracts.Infrastructure.Services;

namespace CreativeCookies.VideoHosting.Infrastructure
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
    }
}
