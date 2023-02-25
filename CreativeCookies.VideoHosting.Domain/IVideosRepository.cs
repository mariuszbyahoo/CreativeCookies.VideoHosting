using CreativeCookies.VideoHosting.Contracts;
using CreativeCookies.VideoHosting.EfCore;

namespace CreativeCookies.VideoHosting.Domain
{
    interface IVideosRepository
    {
        IEnumerable<IVideo> GetAll();
        IVideo GetVideo(Guid id);
        IVideo PostVideo(IVideo video);
        IVideo UpdateVideo(IVideo video);
        void DeleteVideo(Guid id);
    }
}