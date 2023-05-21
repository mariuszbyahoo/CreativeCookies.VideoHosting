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

        public async Task<IFilmsPaginatedResult> GetFilmsPaginatedResult(string search, int pageNumber, int pageSize)
        {
            // Get a reference to your DbContext (replace MyDbContext with your actual DbContext class)
            IQueryable<VideoMetadata> query = _context.VideosMetadata;
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

    }
}
