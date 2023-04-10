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

            // HACK: should this return also binary images?

            // Paginate the blobs
            int totalBlobs = blobs.Count;
            var paginatedBlobs = blobs.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            var result = new List<FilmDto>();
            foreach (var blob in paginatedBlobs)
            {
                var blobClient = filmsClient.GetBlobClient(blob.Name);
                var properties = await blobClient.GetPropertiesAsync();
                var length = properties.Value.Metadata.Count > 0 ? properties.Value.Metadata["length"] : "";
                var name = blob.Name;
                var thumbnailName = blob.Name.Substring(0, blob.Name.LastIndexOf('.'));
                var createdOn = blob.Properties?.CreatedOn?.ToString();
                result.Add(new FilmDto() { Name = name, ThumbnailName = thumbnailName, Length = length, CreatedOn = createdOn });
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
