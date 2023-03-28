using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
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
        public IActionResult GetSasToken()
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var sasToken = GenerateSasToken(containerClient);
            return Ok(new { sasToken });
        }

        private string GenerateSasToken(BlobContainerClient containerClient)
        {
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerClient.Name,
                Resource = "c",
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-300),
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(300),
            };

            sasBuilder.SetPermissions(BlobContainerSasPermissions.List);
            var sasQueryParameters = sasBuilder.ToSasQueryParameters(_storageSharedKeyCredential);
            return sasQueryParameters.ToString();
        }
    }
}
