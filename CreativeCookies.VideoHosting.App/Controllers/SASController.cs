using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Mvc;

namespace CreativeCookies.VideoHosting.App.Controllers
{
    [Route("api/[controller]")]
    public class SASController : ControllerBase
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _filmsContainerName;
        private readonly string _thumbnailsContainerName;
        private readonly StorageSharedKeyCredential _storageSharedKeyCredential;

        public SASController(BlobServiceClient blobServiceClient, StorageSharedKeyCredential storageSharedKeyCredential)
        {
            _blobServiceClient = blobServiceClient;
            _storageSharedKeyCredential = storageSharedKeyCredential;
            _filmsContainerName = "films";
            _thumbnailsContainerName = "thumbnails";
        }
        // MyIpAddress: 79.191.57.150
        [HttpGet("filmsList")]
        public IActionResult GetSasTokenForContainer()
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_filmsContainerName);
            var sasToken = GenerateSasToken(containerClient, EndpointType.ListBlobs);
            return Ok(new { sasToken });
        }

        [HttpGet("film/{blobTitle}")]
        public IActionResult GetSasTokenForFilm(string blobTitle)
        {
            if (string.IsNullOrEmpty(blobTitle))
            {
                return BadRequest($"Field: string blobTitle is mandatory!");
            }
            var containerClient = _blobServiceClient.GetBlobContainerClient(_filmsContainerName);
            var sasToken = GenerateSasToken(containerClient, EndpointType.BlobRead, blobTitle);
            return Ok(new { sasToken });
        }

        [HttpGet("film-upload/{blobTitle}")]
        public IActionResult GetSasTokenForFilmUpload(string blobTitle)
        {
            if (string.IsNullOrEmpty(blobTitle))
            {
                return BadRequest($"Field: string blobTitle is mandatory!");
            }
            var containerClient = _blobServiceClient.GetBlobContainerClient(_filmsContainerName);
            var sasToken = GenerateSasToken(containerClient, EndpointType.BlobUpload, blobTitle);
            return Ok(new { sasToken });
        }

        [HttpGet("thumbnail/{blobTitle}")]
        public IActionResult GetSasTokenForThumbnail(string blobTitle)
        {
            if (string.IsNullOrEmpty(blobTitle))
            {
                return BadRequest($"Field: string blobTitle is mandatory!");
            }
            var containerClient = _blobServiceClient.GetBlobContainerClient(_thumbnailsContainerName);
            var sasToken = GenerateSasToken(containerClient, EndpointType.BlobRead, blobTitle);
            return Ok(new { sasToken });
        }

        [HttpGet("thumbnail-upload/{blobTitle}")]
        public IActionResult GetSasTokenForThumbnailUpload(string blobTitle)
        {
            if (string.IsNullOrEmpty(blobTitle))
            {
                return BadRequest($"Field: string blobTitle is mandatory!");
            }
            var containerClient = _blobServiceClient.GetBlobContainerClient(_thumbnailsContainerName);
            var sasToken = GenerateSasToken(containerClient, EndpointType.BlobRead, blobTitle);
            return Ok(new { sasToken });
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

    public enum EndpointType
    {
        ListBlobs = 0,
        BlobRead = 1,
        BlobUpload = 2
    }
}
