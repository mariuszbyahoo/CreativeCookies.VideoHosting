using CreativeCookies.VideoHosting.Contracts;
using CreativeCookies.VideoHosting.EfCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Domain
{
    public class VideosRepository : IVideosRepository
    {
        private VideoHostingDbContext _context;

        public VideosRepository(VideoHostingDbContext context)
        {
            _context = context;
        }

        public void DeleteVideo(Guid id)
        {
            if(_context.Remove())
        }

        public IEnumerable<IVideo> GetAll()
        {
            throw new NotImplementedException();
        }

        public IVideo GetVideo(Guid id)
        {
            throw new NotImplementedException();
        }

        public IVideo PostVideo(IVideo video)
        {
            throw new NotImplementedException();
        }

        public IVideo UpdateVideo(IVideo video)
        {
            throw new NotImplementedException();
        }
    }
}
