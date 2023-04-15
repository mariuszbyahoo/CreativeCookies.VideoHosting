using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using CreativeCookies.VideoHosting.Contracts.Azure;
using CreativeCookies.VideoHosting.Contracts.ModelContracts;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Domain.Endpoints;
using CreativeCookies.VideoHosting.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Domain.Repositories
{
    public class SasTokenRepository : ISasTokenRepository
    {
        private readonly IBlobServiceClientWrapper _blobServiceClientWrapper;
        private readonly StorageSharedKeyCredential _storageSharedKeyCredential;

        public ISasTokenResult GetSasTokenForContainer(string containerName)
        {
            var containerClient = _blobServiceClientWrapper.GetBlobContainerClient(containerName);
            var sasToken = GenerateSasToken(containerClient, EndpointType.ListBlobs);
            return new SasTokenResult(sasToken);
        }

        public ISasTokenResult GetSasTokenForFilm(string filmName, string containerName)
        {
            var containerClient = _blobServiceClientWrapper.GetBlobContainerClient(containerName);
            var sasToken = GenerateSasToken(containerClient, EndpointType.BlobRead, filmName);
            return new SasTokenResult(sasToken);
        }

        public ISasTokenResult GetSasTokenForFilmUpload(string filmName, string containerName)
        {
            var containerClient = _blobServiceClientWrapper.GetBlobContainerClient(containerName);
            var sasToken = GenerateSasToken(containerClient, EndpointType.BlobUpload, filmName);
            return new SasTokenResult(sasToken);
        }

        public ISasTokenResult GetSasTokenForThumbnail(string thumbnailName, string containerName)
        {
            var containerClient = _blobServiceClientWrapper.GetBlobContainerClient(containerName);
            var sasToken = GenerateSasToken(containerClient, EndpointType.BlobRead, thumbnailName);
            return new SasTokenResult(sasToken);
        }

        public ISasTokenResult GetSasTokenForThumbnailUpload(string thumbnailName, string containerName)
        {
            var containerClient = _blobServiceClientWrapper.GetBlobContainerClient(containerName);
            var sasToken = GenerateSasToken(containerClient, EndpointType.BlobUpload, thumbnailName);
            return new SasTokenResult(sasToken);
        }

        private string GenerateSasToken(BlobContainerClient containerClient, EndpointType endpointType, string blobTitle = "")
        {
            BlobSasBuilder sasBuilder = null;
            if (endpointType == EndpointType.ListBlobs)
            {
                sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerClient.Name,
                    Resource = "c",
                    StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(30),
                };
                sasBuilder.SetPermissions(BlobSasPermissions.List | BlobSasPermissions.Read);
            }
            else
            {
                sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerClient.Name,
                    BlobName = blobTitle,
                    Resource = "b",
                    StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(6000),
                };
                if (endpointType == EndpointType.BlobUpload)
                {
                    sasBuilder.SetPermissions(BlobSasPermissions.Create | BlobSasPermissions.Write);
                }
                else
                {
                    sasBuilder.SetPermissions(BlobSasPermissions.Read);
                }
            }
            var sasQueryParameters = sasBuilder.ToSasQueryParameters(_storageSharedKeyCredential);
            return sasQueryParameters.ToString();
        }

    }
}
