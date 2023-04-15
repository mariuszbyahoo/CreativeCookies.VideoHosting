using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Azure
{
    public interface IBlobServiceClientWrapper
    {
            Task<BlobContainerClient> CreateBlobContainerAsync(string containerName, PublicAccessType publicAccessType = PublicAccessType.None, IDictionary<string, string> metadata = null, CancellationToken cancellationToken = default);

            Task DeleteBlobContainerAsync(string containerName, CancellationToken cancellationToken = default);

            BlobContainerClient GetBlobContainerClient(string containerName);

    }
}
