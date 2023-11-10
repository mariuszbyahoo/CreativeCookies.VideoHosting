using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Infrastructure.Azure
{
    public interface IMyHubBlobService
    {
        Task<bool> DeleteBlob(string blobName, string containerName);
        Task<BlobContentInfo> UploadPdfToAzureAsync(byte[] pdfContent, string fileName);
    }
}
