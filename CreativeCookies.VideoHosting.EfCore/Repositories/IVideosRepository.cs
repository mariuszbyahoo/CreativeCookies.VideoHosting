using CreativeCookies.VideoHosting.Contracts;
using CreativeCookies.VideoHosting.EfCore;

namespace CreativeCookies.VideoHosting.EfCore
{
    interface IVideosRepository
    {
        Task<IEnumerable<IVideo>> GetAll();
        Task<IVideo> GetVideo(Guid id, CancellationToken token);
        Task<IVideo> PostVideo(IVideo video, CancellationToken token);
        Task<IVideo> UpdateVideo(IVideo video, CancellationToken token);
        Task DeleteVideo(Guid id, CancellationToken token);
    }
}