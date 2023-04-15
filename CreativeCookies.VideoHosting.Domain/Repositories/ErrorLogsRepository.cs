using CreativeCookies.VideoHosting.Contracts.ModelContracts;
using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.DAL.DTOs;
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

        public IEnumerable<IErrorLog> GetErrorLogs()
        {
            var res = _ctx.ClientErrors.ToList();
            return res;
        }

        public async Task<IErrorLog> LogNewError(string errorLog)
        {
            var newError = new ClientException() { Id = Guid.NewGuid(), Log = errorLog };

            var res = await _ctx.AddAsync(newError);
            await _ctx.SaveChangesAsync();
            return res?.Entity as IErrorLog;
        }
    }
}
