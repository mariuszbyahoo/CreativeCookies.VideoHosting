using CreativeCookies.VideoHosting.DTOs;

namespace CreativeCookies.VideoHosting.Contracts.Repositories
{
    public interface IErrorLogsRepository
    {
        Task<IEnumerable<ErrorLogDto>> GetErrorLogs();

        Task<ErrorLogDto> SaveNewLog(string errorLog);
    }
}
