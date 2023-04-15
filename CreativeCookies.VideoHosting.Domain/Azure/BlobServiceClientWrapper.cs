using Azure.Storage.Blobs;
using CreativeCookies.VideoHosting.Contracts.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Domain.Azure
{
    public class BlobServiceClientWrapper : IBlobServiceClientWrapper
    {
        private readonly BlobServiceClient _client;

        public BlobServiceClientWrapper(BlobServiceClient client)
        {
            _client = client;
        }
        public async Task<global::Azure.Storage.Blobs.BlobContainerClient> CreateBlobContainerAsync(string containerName, global::Azure.Storage.Blobs.Models.PublicAccessType publicAccessType = 0, IDictionary<string, string> metadata = null, CancellationToken cancellationToken = default)
        {
            return await _client.CreateBlobContainerAsync(containerName, publicAccessType, metadata, cancellationToken);
        }

        public async Task DeleteBlobContainerAsync(string containerName, CancellationToken cancellationToken = default)
        {
            await _client.DeleteBlobContainerAsync(containerName, null, cancellationToken);
        }

        public global::Azure.Storage.Blobs.BlobContainerClient GetBlobContainerClient(string containerName)
        {
            return _client.GetBlobContainerClient(containerName);
        }
    }
}
