using Azure.Storage.Blobs.Models;
using CreativeCookies.VideoHosting.Contracts.Azure;
using CreativeCookies.VideoHosting.Contracts.DTOs;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.Domain.DTOs;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CreativeCookies.VideoHosting.DAL.DAOs;
using System.Runtime.InteropServices;

namespace CreativeCookies.VideoHosting.Domain.Repositories
{
    public class FilmsRepository : IFilmsRepository
    {
        private readonly string _filmsContainerName;
        private readonly string _thumbnailsContainerName;
        private readonly IBlobServiceClientWrapper _blobServiceClient;
        private readonly AppDbContext _context;

        public FilmsRepository(IBlobServiceClientWrapper wrapper, AppDbContext context) 
        {
            _filmsContainerName = "films";
            _thumbnailsContainerName = "thumbnails";
            _blobServiceClient = wrapper;
            _context = context;
        }

        public async Task<IVideoMetadata> GetVideoMetadata(Guid Id)
        {
            var res = await _context.VideosMetadata.Where(v => v.Id.Equals(Id)).FirstOrDefaultAsync();
            return res;
        }

        public async Task<IFilmsPaginatedResult> GetFilmsPaginatedResult(string search, int pageNumber, int pageSize)
        {
            IQueryable<DAL.DAOs.VideoMetadata> query = _context.VideosMetadata;
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(v => v.Name.ToLower().Contains(search.ToLower()));
            }

            query = query.OrderByDescending(v => v.CreatedOn);

            // Paginate the results
            int totalVideos = await query.CountAsync();
            var paginatedVideos = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            var filmTiles = paginatedVideos.Select(v => new FilmTile()
            {
                Id = v.Id,
                Name = v.Name,
                Description = v.Description,
                ThumbnailName = v.ThumbnailName,
                Length = v.Length,
                CreatedOn = v.CreatedOn.ToString(),
                BlobUrl = v.BlobUrl
            }).ToList();

            var result = new FilmsPaginatedResult()
            {
                Films = filmTiles,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling((double)totalVideos / pageSize),
                HasMore = pageNumber * pageSize < totalVideos
            };
            return result;
        }

        public async Task<IVideoMetadata> SaveVideoMetadata(IVideoMetadata metadata)
        {
            var dao = new DAL.DAOs.VideoMetadata() { 
                Id = metadata.Id, BlobUrl = metadata.BlobUrl, CreatedOn = metadata.CreatedOn, Description = metadata.Description, 
                Length = metadata.Length, Name = metadata.Name, ThumbnailName = metadata.ThumbnailName, VideoType = metadata.VideoType };
            _context.VideosMetadata.Add(dao);
            await _context.SaveChangesAsync();
            return dao;
        }

        public async Task<IVideoMetadata> EditVideoMetadata(IVideoMetadata metadata)
        {
            // Find the video metadata by ID.
            var videoMetadataToUpdate = await _context.VideosMetadata.FindAsync(metadata.Id);

            if (videoMetadataToUpdate != null)
            {
                // Update the Name and Description fields
                videoMetadataToUpdate.Name = metadata.Name;
                videoMetadataToUpdate.Description = metadata.Description;

                // Save the changes to the database.
                await _context.SaveChangesAsync();

                return videoMetadataToUpdate;
            }

            return null;
        }

        public async Task DeleteVideoBlobWithMetadata(Guid Id)
        {
            // Fetch the video metadata from the database
            var videoMetadata = await _context.VideosMetadata.Where(v => v.Id.Equals(Id)).FirstOrDefaultAsync();
            // Delete the video blob from Azure Blob Storage
            await DeleteVideoWithThumbnail(Id);
            if (videoMetadata != null)
            {
                // Delete the video metadata from the database
                _context.VideosMetadata.Remove(videoMetadata);
                await _context.SaveChangesAsync();
            }
        }

        private async Task DeleteVideoWithThumbnail(Guid Id)
        {
            var filmsContainerClient = _blobServiceClient.GetBlobContainerClient(_filmsContainerName);
            var thumbnailsContainerClient = _blobServiceClient.GetBlobContainerClient(_thumbnailsContainerName);
            var videoBlobClient = filmsContainerClient.GetBlobClient($"{Id.ToString().ToUpperInvariant()}.mp4");
            var thumbnailBlobClient = thumbnailsContainerClient.GetBlobClient($"{Id.ToString().ToUpperInvariant()}.jpg");
            var videExists = await videoBlobClient.ExistsAsync();
            if (videExists.Value)
            {
                var result = await videoBlobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
            }
            var thumbnailExists = await thumbnailBlobClient.ExistsAsync();
            if(thumbnailExists.Value)
            {
                var result = await thumbnailBlobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
            }
        }
    }
}
