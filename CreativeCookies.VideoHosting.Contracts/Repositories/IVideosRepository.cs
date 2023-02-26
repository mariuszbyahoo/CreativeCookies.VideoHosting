using CreativeCookies.VideoHosting.Contracts.Models;

namespace CreativeCookies.VideoHosting.Contracts.Repositories
{
    public interface IVideosRepository
    {
        Task<IEnumerable<IVideo>> GetAll(CancellationToken token);
        Task<IVideo> GetVideo(Guid id, CancellationToken token);
        Task<IVideo> PostVideo(IVideo video, CancellationToken token);
        Task<IVideo> UpdateVideo(IVideo video, CancellationToken token);
        Task DeleteVideo(Guid id, CancellationToken token);
    }
}