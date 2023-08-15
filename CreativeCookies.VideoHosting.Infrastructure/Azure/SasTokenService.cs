using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Azure.Storage;
using CreativeCookies.VideoHosting.Contracts.Azure;
using CreativeCookies.VideoHosting.DTOs.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CreativeCookies.VideoHosting.Infrastructure.Enums;
using CreativeCookies.VideoHosting.Contracts.Infrastructure.Stripe;

namespace CreativeCookies.VideoHosting.Infrastructure.Azure
{
    public class SasTokenService : ISasTokenService
    {
        private readonly IBlobServiceClientWrapper _blobServiceClientWrapper;
        private readonly StorageSharedKeyCredential _storageSharedKeyCredential;
        private readonly string _filmsContainerName;
        private readonly string _thumbnailsContainerName;

        public SasTokenService(IBlobServiceClientWrapper blobServiceClientWrapper, StorageSharedKeyCredential storageSharedKeyCredential)
        {
            _blobServiceClientWrapper = blobServiceClientWrapper;
            _storageSharedKeyCredential = storageSharedKeyCredential;
            _filmsContainerName = "films";
            _thumbnailsContainerName = "thumbnails";
        }

        public SasTokenResultDto GetSasTokenForContainer(string containerName)
        {
            var containerClient = _blobServiceClientWrapper.GetBlobContainerClient(containerName);
            var sasToken = GenerateSasToken(containerClient, SasTokenEndpointType.ListBlobs);
            return new SasTokenResultDto(sasToken);
        }

        public SasTokenResultDto GetSasTokenForFilm(string filmName)
        {
            var containerClient = _blobServiceClientWrapper.GetBlobContainerClient(_filmsContainerName);
            var sasToken = GenerateSasToken(containerClient, SasTokenEndpointType.BlobRead, filmName);
            return new SasTokenResultDto(sasToken);
        }

        public SasTokenResultDto GetSasTokenForFilmUpload(string filmName)
        {
            var containerClient = _blobServiceClientWrapper.GetBlobContainerClient(_filmsContainerName);
            var sasToken = GenerateSasToken(containerClient, SasTokenEndpointType.BlobUpload, filmName);
            return new SasTokenResultDto(sasToken);
        }

        public SasTokenResultDto GetSasTokenForThumbnail(string thumbnailName)
        {
            var containerClient = _blobServiceClientWrapper.GetBlobContainerClient(_thumbnailsContainerName);
            var sasToken = GenerateSasToken(containerClient, SasTokenEndpointType.BlobRead, thumbnailName);
            return new SasTokenResultDto(sasToken);
        }

        public SasTokenResultDto GetSasTokenForThumbnailUpload(string thumbnailName)
        {
            var containerClient = _blobServiceClientWrapper.GetBlobContainerClient(_thumbnailsContainerName);
            var sasToken = GenerateSasToken(containerClient, SasTokenEndpointType.BlobUpload, thumbnailName);
            return new SasTokenResultDto(sasToken);
        }

        private string GenerateSasToken(BlobContainerClient containerClient, SasTokenEndpointType endpointType, string blobTitle = "")
        {
            BlobSasBuilder sasBuilder = null;
            if (endpointType == SasTokenEndpointType.ListBlobs)
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
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(120),
                };
                if (endpointType == SasTokenEndpointType.BlobUpload)
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
