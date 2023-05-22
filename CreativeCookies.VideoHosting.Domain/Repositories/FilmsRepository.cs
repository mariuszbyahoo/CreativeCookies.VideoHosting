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

        public async Task<IBlobUrlResult> GetBlobUrl(Guid Id)
        {
            var res = (await _context.VideosMetadata.Where(v => v.Id.Equals(Id)).FirstOrDefaultAsync())?.BlobUrl;
            if (res != null)
            {
                return new BlobUrlResult(res);
            }
            return new BlobUrlResult("NOT_FOUND_IN_REPO");
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

        public async Task SaveVideoMetadata(IVideoMetadata metadata)
        {
            var dao = new DAL.DAOs.VideoMetadata() { 
                BlobUrl = metadata.BlobUrl, CreatedOn = metadata.CreatedOn, Description = metadata.Description, Length = metadata.Length, 
                Name = metadata.Name, ThumbnailName = metadata.ThumbnailName, VideoType = metadata.VideoType };
            _context.VideosMetadata.Add(dao);
            await _context.SaveChangesAsync();
        }
    }
}
