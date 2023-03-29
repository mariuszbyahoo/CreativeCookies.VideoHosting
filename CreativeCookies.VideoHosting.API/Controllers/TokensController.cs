using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using CreativeCookies.VideoHosting.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CreativeCookies.VideoHosting.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokensController : ControllerBase
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;
        private readonly StorageSharedKeyCredential _storageSharedKeyCredential;

        public TokensController(BlobServiceClient blobServiceClient, StorageSharedKeyCredential storageSharedKeyCredential)
        {
            _blobServiceClient = blobServiceClient;
            _storageSharedKeyCredential = storageSharedKeyCredential;
            _containerName = "films";
        }
        // MyIpAddress: 79.191.57.150
        [HttpGet("container")]
        public IActionResult GetSasTokenForContainer()
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var sasToken = GenerateSasToken(containerClient, EndpointType.Container);
            return Ok(new { sasToken });
        }

        [HttpGet("film")]
        public IActionResult GetSasTokenForFilm([FromQuery] string blobTitle)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var sasToken = GenerateSasToken(containerClient, EndpointType.Film, blobTitle);
            return Ok(new { sasToken });
        }

        private string GenerateSasToken(BlobContainerClient containerClient, EndpointType endpointType, string blobTitle = "")
        {
            BlobSasBuilder sasBuilder = null;
            if (endpointType == EndpointType.Container)
            {
                sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerClient.Name,
                    Resource = "c",
                    StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(30),
                };
            }
            else
            {
                sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerClient.Name,
                    BlobName = blobTitle,
                    Resource =  "b",
                    StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(1),
                };
            }

            sasBuilder.SetPermissions(endpointType == EndpointType.Container ? BlobContainerSasPermissions.List : BlobContainerSasPermissions.Read);
            var sasQueryParameters = sasBuilder.ToSasQueryParameters(_storageSharedKeyCredential);
            return sasQueryParameters.ToString();
        }
    }
}
