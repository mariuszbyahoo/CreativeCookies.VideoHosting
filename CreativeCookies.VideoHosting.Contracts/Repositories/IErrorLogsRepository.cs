using CreativeCookies.VideoHosting.Contracts.DTOs;

namespace CreativeCookies.VideoHosting.Contracts.Repositories
{
    public interface IErrorLogsRepository
    {
        IEnumerable<IErrorLog> GetErrorLogs();

        Task<IErrorLog> LogNewError(string errorLog);
    }
}
