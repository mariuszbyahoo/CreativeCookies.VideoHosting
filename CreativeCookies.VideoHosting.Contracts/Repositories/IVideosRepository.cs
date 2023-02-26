using CreativeCookies.VideoHosting.Contracts.Models;

namespace CreativeCookies.VideoHosting.Contracts.Repositories
{
    public interface IVideosRepository
    {
        /// <summary>
        /// Method performing lookup to search for a video with supplied id in the database.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns>True, if such an entity has been found and otherwise, false</returns>
        Task<bool> IsPresentInDatabase(Guid id, CancellationToken token);
        Task<IEnumerable<IVideo>> GetAll(CancellationToken token);
        Task<IVideo> GetVideo(Guid id, CancellationToken token);
        Task<IVideo> PostVideo(IVideo video, CancellationToken token);
        Task<IVideo> UpdateVideo(IVideo video, CancellationToken token);
        Task DeleteVideo(Guid id, CancellationToken token);
    }
}