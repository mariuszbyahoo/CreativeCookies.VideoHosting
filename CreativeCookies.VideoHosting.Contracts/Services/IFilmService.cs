using CreativeCookies.VideoHosting.DTOs.Films;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Services
{
    public interface IFilmService
    {
        public Task<FilmsPaginatedResultDto> GetFilmsPaginatedResult(string search, int pageNumber, int pageSize);
        public Task<DTOs.Films.VideoMetadataDto> GetVideoMetadata(Guid Id);
        public Task<DTOs.Films.VideoMetadataDto> SaveVideoMetadata(DTOs.Films.VideoMetadataDto metadata);
        public Task<DTOs.Films.VideoMetadataDto> EditVideoMetadata(DTOs.Films.VideoMetadataDto metadata);
        public Task DeleteVideoBlobWithMetadata(Guid Id);
    }
}
