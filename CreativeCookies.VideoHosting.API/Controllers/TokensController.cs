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
        private readonly string _storageAccountConnectionString;
        private readonly string _storageAccountKey;
        public TokensController()
        {
            // HACK: TODO Move to secrets or into other place
            _containerName = "mytubestoragecool";
            _storageAccountConnectionString = "DefaultEndpointsProtocol=https;AccountName=mytubestoragecool;AccountKey=Sb4becrL9oYYzT/HfLJ8iP72VOgLtZNyc990W5NtK8ykxiVHc9rKRxw3zskMcCONz6t03XOzseZL+AStVWANoQ==;EndpointSuffix=core.windows.net";
            _storageAccountKey = "Sb4becrL9oYYzT/HfLJ8iP72VOgLtZNyc990W5NtK8ykxiVHc9rKRxw3zskMcCONz6t03XOzseZL+AStVWANoQ==";
            _blobServiceClient = new BlobServiceClient(_storageAccountConnectionString);

        }

        [HttpGet("container")]
        public IActionResult GetSasToken()
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var sasToken = GenerateSasToken(containerClient);
            return Ok(new { SasToken = sasToken });
        }

        private string GenerateSasToken(BlobContainerClient containerClient)
        {
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerClient.Name,
                Resource = "c",
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            sasBuilder.SetPermissions(BlobContainerSasPermissions.List);

            var storageSharedKeyCredential = new StorageSharedKeyCredential(containerClient.AccountName, _storageAccountKey);

            var sasToken = sasBuilder.ToSasQueryParameters(storageSharedKeyCredential).ToString();
            return sasToken;
        }
    }
}
