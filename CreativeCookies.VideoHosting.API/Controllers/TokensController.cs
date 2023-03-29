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
        public IActionResult GetSasTokenForFilm()
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var sasToken = GenerateSasToken(containerClient, EndpointType.Film);
            return Ok(new { sasToken });
        }

        private string GenerateSasToken(BlobContainerClient containerClient, EndpointType endpointType)
        {
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerClient.Name,
                Resource = endpointType == EndpointType.Container ? "c" : "b",
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(30),
            };

            sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);
            var sasQueryParameters = sasBuilder.ToSasQueryParameters(_storageSharedKeyCredential);
            return sasQueryParameters.ToString();
        }
    }
}
