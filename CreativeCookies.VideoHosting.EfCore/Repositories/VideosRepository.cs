using CreativeCookies.VideoHosting.Contracts.Models;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.EfCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.EfCore.Repositories
{
    public class VideosRepository : IVideosRepository
    {
        private readonly VideoHostingDbContext _context;

        public VideosRepository(VideoHostingDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsPresentInDatabase(Guid id, CancellationToken token)
        {
            return await _context.Videos.Where(x => x.Id == id).FirstOrDefaultAsync(token) != null;
        }

        public async Task DeleteVideo(Guid id, CancellationToken token)
        {
            var entity = await  _context.Videos.Where(v => v.Id.Equals(id)).FirstOrDefaultAsync(token);
            if (entity != null) {
                _context.Remove(entity);
                await _context.SaveChangesAsync(token);
            }
        }

        public async Task<IEnumerable<IVideo>> GetAll(CancellationToken token)
        {
            return await _context.Videos.ToListAsync(token);
        }

        public async Task<IVideo> GetVideo(Guid id, CancellationToken token)
        {
            return await _context.Videos.Where(v => v.Id.Equals(id)).FirstOrDefaultAsync(token);
        }

        public async Task<IVideo> PostVideo(IVideo video, CancellationToken token = default)
        {
            var res = await _context.AddAsync(video, token);
            await _context.SaveChangesAsync(token);
            return res?.Entity;
        }

        public async Task<IVideo> UpdateVideo(IVideo video, CancellationToken token)
        {
            var res = _context.Update(video);
            await _context.SaveChangesAsync(token);
            return res?.Entity;
        }
    }
}
