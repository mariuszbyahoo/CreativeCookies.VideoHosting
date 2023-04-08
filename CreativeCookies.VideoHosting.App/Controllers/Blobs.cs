using Azure.Storage.Blobs;
using Azure.Storage;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs.Models;

namespace CreativeCookies.VideoHosting.App.Controllers
{
    [Route("api/[controller]")]
    public class BlobsController : ControllerBase
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _filmsContainerName;
        private readonly string _thumbnailsContainerName;
        private readonly StorageSharedKeyCredential _storageSharedKeyCredential;

        public BlobsController (BlobServiceClient blobServiceClient, StorageSharedKeyCredential storageSharedKeyCredential)
        {
            _blobServiceClient = blobServiceClient;
            _storageSharedKeyCredential = storageSharedKeyCredential;
            _filmsContainerName = "films";
            _thumbnailsContainerName = "thumbnails";
        }

        [Route("films")]
        public async Task<IActionResult> GetFilms([FromQuery] string search = "", int pageNumber = 1, int pageSize = 30)
        {
            var result = new List<string>();

            var filmsClient  = _blobServiceClient.GetBlobContainerClient(_filmsContainerName);
            var thumbnailsClient  = _blobServiceClient.GetBlobContainerClient(_thumbnailsContainerName);

            List<BlobItem> blobs = new List<BlobItem>();
            await foreach (BlobItem blob in filmsClient.GetBlobsAsync())
            {
                blobs.Add(blob);
            }

            // Filter the blobs based on the search term (if provided)
            if (!string.IsNullOrEmpty(search))
            {
                blobs = blobs.Where(b => b.Name.ToLower().Contains(search.ToLower())).ToList();
            }

            blobs = blobs.OrderByDescending(b => b.Properties.CreatedOn).ToList();

            // Paginate the blobs
            int totalBlobs = blobs.Count;
            var paginatedBlobs = blobs.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new
            {
                films = paginatedBlobs,
                currentPage = pageNumber,
                totalPages = (int)Math.Ceiling((double)totalBlobs / pageSize),
                hasMore = pageNumber * pageSize < totalBlobs
            });
        }
    }
}
