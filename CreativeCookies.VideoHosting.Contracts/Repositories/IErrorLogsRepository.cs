using CreativeCookies.VideoHosting.DTOs;

namespace CreativeCookies.VideoHosting.Contracts.Repositories
{
    public interface IErrorLogsRepository
    {
        IEnumerable<ErrorLogDto> GetErrorLogs();

        Task<ErrorLogDto> LogNewError(string errorLog);
    }
}
