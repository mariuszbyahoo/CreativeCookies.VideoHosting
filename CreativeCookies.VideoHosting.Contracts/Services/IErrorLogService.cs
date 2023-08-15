using CreativeCookies.VideoHosting.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Services
{
    public interface IErrorLogsService
    {
        Task<IEnumerable<ErrorLogDto>> GetErrorLogs();

        Task<ErrorLogDto> AddNewLog(string errorLog);
    }
}
