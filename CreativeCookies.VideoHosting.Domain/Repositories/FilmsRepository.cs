using Azure.Storage.Blobs.Models;
using CreativeCookies.VideoHosting.Contracts.Azure;
using CreativeCookies.VideoHosting.Contracts.DTOs;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Domain.Repositories
{
    public class FilmsRepository : IFilmsRepository
    {
        private readonly string _filmsContainerName;
        private readonly string _thumbnailsContainerName;
        private readonly IBlobServiceClientWrapper _blobServiceClient;

        public FilmsRepository(IBlobServiceClientWrapper wrapper) 
        {
            _filmsContainerName = "films";
            _thumbnailsContainerName = "thumbnails";
            _blobServiceClient = wrapper;
        }

        public async Task<IFilmsPaginatedResult> GetFilmsPaginatedResult(string search, int pageNumber, int pageSize)
        {
            var filmsClient = _blobServiceClient.GetBlobContainerClient(_filmsContainerName);
            var thumbnailsClient = _blobServiceClient.GetBlobContainerClient(_thumbnailsContainerName);

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

            var filmTiles = new List<FilmTile>();
            foreach (var blob in paginatedBlobs)
            {
                try
                {
                    var blobClient = filmsClient.GetBlobClient(blob.Name);
                    var properties = await blobClient.GetPropertiesAsync();
                    var length = properties.Value.Metadata.Count > 0 ? properties.Value.Metadata["length"] : "";
                    var name = blob.Name;
                    var createdOn = blob.Properties?.CreatedOn?.ToString() ?? string.Empty;
                    var imageBlob = thumbnailBlobs.FirstOrDefault(b => b.Name.Substring(0, b.Name.LastIndexOf('.')).Equals(name.Substring(0, name.LastIndexOf('.')), StringComparison.OrdinalIgnoreCase));
                    filmTiles.Add(new FilmTile() { Name = name, ThumbnailName = imageBlob?.Name, Length = length, CreatedOn = createdOn });
                }
                catch (Exception ex)
                {
                    throw new Exception($"Exception thrown for blob: {blob.Name}", ex);
                }
            }
            var result = new FilmsPaginatedResult() {
                Films = filmTiles,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling((double)totalBlobs / pageSize),
                HasMore = pageNumber * pageSize < totalBlobs
            };
            return result;
        }
    }
}
