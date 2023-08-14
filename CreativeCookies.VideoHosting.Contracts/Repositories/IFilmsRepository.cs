using CreativeCookies.VideoHosting.DTOs.Films;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Repositories
{
    public interface IFilmsRepository
    {
        Task<VideoMetadataDto> GetVideoMetadata(Guid videoId);
        Task<FilmsPaginatedResultDto> GetFilmsPaginatedResult(string search, int pageNumber, int pageSize);
        Task<VideoMetadataDto> SaveVideoMetadata(VideoMetadataDto metadata);
        Task<VideoMetadataDto> EditVideoMetadata(VideoMetadataDto metadata);
        Task DeleteVideoBlobWithMetadata(Guid id);
    }
}
