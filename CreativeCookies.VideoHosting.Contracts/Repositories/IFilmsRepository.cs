using CreativeCookies.VideoHosting.Contracts.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Repositories
{
    public interface IFilmsRepository
    {
        Task<IVideoMetadata> GetVideoMetadata(Guid videoId);
        Task<IFilmsPaginatedResult> GetFilmsPaginatedResult(string search, int pageNumber, int pageSize);
        Task<IVideoMetadata> SaveVideoMetadata(IVideoMetadata metadata);
        Task<IVideoMetadata> EditVideoMetadata(IVideoMetadata metadata);
        Task DeleteVideoBlobWithMetadata(Guid id);
    }
}
