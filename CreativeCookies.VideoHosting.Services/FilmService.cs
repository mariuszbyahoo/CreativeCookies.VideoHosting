using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.Services;
using CreativeCookies.VideoHosting.DTOs.Films;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Services
{
    public class FilmService : IFilmService
    {
        private readonly IFilmsRepository _repo;
        private readonly IMyHubBlobService _myHubBlobService;
        private readonly string _filmsContainerName;
        private readonly string _thumbnailsContainerName;
        public FilmService(IFilmsRepository repo, IMyHubBlobService myHubBlobService)
        {
            _repo = repo;
            _myHubBlobService = myHubBlobService;
            _filmsContainerName = "films";
            _thumbnailsContainerName = "thumbnails";
        }

        public async Task DeleteVideoBlobWithMetadata(Guid Id)
        {
            await _repo.DeleteVideoMetadata(Id);
            await _myHubBlobService.DeleteBlob($"{Id.ToString().ToUpperInvariant()}.mp4", _filmsContainerName);
            await _myHubBlobService.DeleteBlob($"{Id.ToString().ToUpperInvariant()}.jpg", _thumbnailsContainerName);
        }

        public async Task<VideoMetadataDto> EditVideoMetadata(VideoMetadataDto metadata)
        {
            var result = await _repo.EditVideoMetadata(metadata);
            return result;
        }

        public async Task<FilmsPaginatedResultDto> GetFilmsPaginatedResult(string search, int pageNumber, int pageSize)
        {
            var result = await _repo.GetFilmsPaginatedResult(search, pageNumber, pageSize);
            return result;
        }

        public async Task<VideoMetadataDto> GetVideoMetadata(Guid Id)
        {
            var metadata = await _repo.GetVideoMetadata(Id);
            return metadata;
        }

        public async Task<VideoMetadataDto> SaveVideoMetadata(VideoMetadataDto metadata)
        {
            var result = await _repo.SaveVideoMetadata(metadata);
            return result;
        }
    }
}
