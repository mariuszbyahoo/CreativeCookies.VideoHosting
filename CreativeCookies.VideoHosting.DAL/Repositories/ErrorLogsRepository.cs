using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.DAL.DAOs;
using CreativeCookies.VideoHosting.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CreativeCookies.VideoHosting.DAL.Repositories
{
    public class ErrorLogsRepository : IErrorLogsRepository
    {
        private AppDbContext _ctx;
        public ErrorLogsRepository(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<IEnumerable<ErrorLogDto>> GetErrorLogs()
        {
            var res = await _ctx.ClientErrors.ToListAsync();
            return res.Cast<ErrorLogDto>();
        }

        public async Task<ErrorLogDto> SaveNewLog(string errorLog)
        {
            var newError = new ClientException() { Id = Guid.NewGuid(), Log = errorLog };

            var res = await _ctx.AddAsync(newError);
            _ctx.SaveChanges();
            if (res == null) return null;
            else
            {
                return new ErrorLogDto(res.Entity.Log);
            }
        }
    }
}
