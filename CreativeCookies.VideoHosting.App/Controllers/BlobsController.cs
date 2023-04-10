using Azure.Storage.Blobs;
using Azure.Storage;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs.Models;
using CreativeCookies.VideoHosting.App.Models;

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
        public async Task<IActionResult> GetFilms([FromQuery] string search = "", int pageNumber = 1, int pageSize = 24)
        {
            var filmsClient  = _blobServiceClient.GetBlobContainerClient(_filmsContainerName);
            var thumbnailsClient  = _blobServiceClient.GetBlobContainerClient(_thumbnailsContainerName);

            List<BlobItem> filmBlobs = new List<BlobItem>();
            List<BlobItem> thumbnailBlobs = new List<BlobItem>();
            await foreach (BlobItem blob in filmsClient.GetBlobsAsync())
            {
                filmBlobs.Add(blob);
            }
            await foreach (BlobItem blob in thumbnailsClient.GetBlobsAsync())
            {
                thumbnailBlobs.Add(blob);
            }

            // Filter the blobs based on the search term (if provided)
            if (!string.IsNullOrEmpty(search))
            {
                filmBlobs = filmBlobs.Where(b => b.Name.ToLower().Contains(search.ToLower())).ToList();
            }

            filmBlobs = filmBlobs.OrderByDescending(b => b.Properties.CreatedOn).ToList();

            // Paginate the blobs
            int totalBlobs = filmBlobs.Count;
            var paginatedBlobs = filmBlobs.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            var result = new List<FilmDto>();
            foreach (var blob in paginatedBlobs)
            {
                try
                {
                    var blobClient = filmsClient.GetBlobClient(blob.Name);
                    var properties = await blobClient.GetPropertiesAsync();
                    var length = properties.Value.Metadata.Count > 0 ? properties.Value.Metadata["length"] : "";
                    var name = blob.Name;
                    var createdOn = blob.Properties?.CreatedOn?.ToString();
                    var imageBlob = thumbnailBlobs.FirstOrDefault(b => b.Name.Substring(0, b.Name.LastIndexOf('.')).Equals(name.Substring(0, name.LastIndexOf('.')), StringComparison.OrdinalIgnoreCase));
                    result.Add(new FilmDto() { Name = name, ThumbnailName = imageBlob?.Name, Length = length, CreatedOn = createdOn });
                }
                catch(Exception ex)
                {
                    throw new Exception($"Exception thrown for blob: {blob.Name}", ex);
                }
            }

            return Ok(new
            {
                films = result,
                currentPage = pageNumber,
                totalPages = (int)Math.Ceiling((double)totalBlobs / pageSize),
                hasMore = pageNumber * pageSize < totalBlobs
            });
        }
    }
}
