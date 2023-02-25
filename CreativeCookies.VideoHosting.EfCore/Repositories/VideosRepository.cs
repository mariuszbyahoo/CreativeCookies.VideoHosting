using CreativeCookies.VideoHosting.Contracts;
using CreativeCookies.VideoHosting.EfCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.EfCore
{
    public class VideosRepository : IVideosRepository
    {
        private VideoHostingDbContext _context;

        public VideosRepository(VideoHostingDbContext context)
        {
            _context = context;
        }

        public async Task DeleteVideo(Guid id, CancellationToken token)
        {
            var entity = await  _context.Videos.FirstOrDefaultAsync(v => v.Id.Equals(id), token);
            if (entity != null) {
                _context.Remove(entity);
                await _context.SaveChangesAsync(token);
            }
        }

        public async Task<IEnumerable<IVideo>> GetAll()
        {
            return await _context.Videos.ToListAsync();
        }

        public async Task<IVideo> GetVideo(Guid id, CancellationToken token)
        {
            return await _context.Videos.FirstOrDefaultAsync(v => v.Id.Equals(id), token);
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
