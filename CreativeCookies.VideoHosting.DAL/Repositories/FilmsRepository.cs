using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.DAL.Contexts;
using Microsoft.EntityFrameworkCore;
using CreativeCookies.VideoHosting.DTOs.Films;

namespace CreativeCookies.VideoHosting.DAL.Repositories
{
    public class FilmsRepository : IFilmsRepository
    {
        private readonly AppDbContext _context;

        public FilmsRepository(AppDbContext context) 
        {
            _context = context;
        }

        public async Task<VideoMetadataDto> GetVideoMetadata(Guid Id)
        {
            var response = await _context.VideosMetadata.Where(v => v.Id.Equals(Id)).FirstOrDefaultAsync();

            var result = new VideoMetadataDto(response.Id, response.Name, response.Description, response.Length, response.ThumbnailName, response.BlobUrl, response.VideoType, response.CreatedOn);
            return result;
        }

        public async Task<FilmsPaginatedResultDto> GetFilmsPaginatedResult(string search, int pageNumber, int pageSize)
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

            var filmTiles = paginatedVideos.Select(v => new FilmTileDto(v.Id, v.Name, v.Description, v.ThumbnailName, v.Length, v.CreatedOn.ToString(), v.BlobUrl)).ToList();

            var result = new FilmsPaginatedResultDto()
            {
                Films = filmTiles,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling((double)totalVideos / pageSize),
                HasMore = pageNumber * pageSize < totalVideos
            };
            return result;
        }

        public async Task<DTOs.Films.VideoMetadataDto> SaveVideoMetadata(DTOs.Films.VideoMetadataDto metadata)
        {
            var dao = new DAL.DAOs.VideoMetadata() { 
                Id = metadata.Id, BlobUrl = metadata.BlobUrl, CreatedOn = metadata.CreatedOn, Description = metadata.Description, 
                Length = metadata.Length, Name = metadata.Name, ThumbnailName = metadata.ThumbnailName, VideoType = metadata.VideoType };
            _context.VideosMetadata.Add(dao);
            await _context.SaveChangesAsync();
            var result = new VideoMetadataDto(dao.Id, dao.Name, dao.Description, dao.Length, dao.ThumbnailName, dao.BlobUrl, dao.VideoType, dao.CreatedOn);
            return result;
        }

        public async Task<DTOs.Films.VideoMetadataDto> EditVideoMetadata(DTOs.Films.VideoMetadataDto metadata)
        {
            // Find the video metadata by ID.
            var videoMetadataToUpdate = await _context.VideosMetadata.FindAsync(metadata.Id);

            if (videoMetadataToUpdate != null)
            {
                // Update the Name and Description fields
                videoMetadataToUpdate.Name = metadata.Name;
                videoMetadataToUpdate.Description = metadata.Description;

                // Save the changes to the database.
                var status = await _context.SaveChangesAsync();
                if(status > 0) 
                    return metadata;
            }

            return null;
        }

        public async Task DeleteVideoMetadata(Guid Id)
        {
            // Fetch the video metadata from the database
            var videoMetadata = await _context.VideosMetadata.Where(v => v.Id.Equals(Id)).FirstOrDefaultAsync();
            if (videoMetadata != null)
            {
                // Delete the video metadata from the database
                _context.VideosMetadata.Remove(videoMetadata);
                await _context.SaveChangesAsync();
            }
        }
    }
}
