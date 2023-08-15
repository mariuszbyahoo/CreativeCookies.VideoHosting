using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.Services;
using CreativeCookies.VideoHosting.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Services
{
    public class ErrorLogsService : IErrorLogsService
    {
        private readonly IErrorLogsRepository _repo;

        public ErrorLogsService(IErrorLogsRepository repo)
        {
            _repo = repo;
        }

        public async Task<ErrorLogDto> AddNewLog(string errorLog)
        {
            return await _repo.SaveNewLog(errorLog);
        }

        public async Task<IEnumerable<ErrorLogDto>> GetErrorLogs()
        {
            return await _repo.GetErrorLogs();
        }
    }
}
