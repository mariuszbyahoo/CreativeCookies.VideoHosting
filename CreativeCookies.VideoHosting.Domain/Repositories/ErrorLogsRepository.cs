using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.DAL.DAOs;
using CreativeCookies.VideoHosting.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Domain.Repositories
{
    public class ErrorLogsRepository : IErrorLogsRepository
    {
        private AppDbContext _ctx;
        public ErrorLogsRepository(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public IEnumerable<ErrorLogDto> GetErrorLogs()
        {
            var res = _ctx.ClientErrors.ToList();
            return res.Cast<ErrorLogDto>();
        }

        public async Task<ErrorLogDto> LogNewError(string errorLog)
        {
            var newError = new ClientException() { Id = Guid.NewGuid(), Log = errorLog };

            var res = await _ctx.AddAsync(newError);
            await _ctx.SaveChangesAsync();
            if (res == null) return null;
            else
            {
                return new ErrorLogDto(res.Entity.Log);
            }
        }
    }
}
