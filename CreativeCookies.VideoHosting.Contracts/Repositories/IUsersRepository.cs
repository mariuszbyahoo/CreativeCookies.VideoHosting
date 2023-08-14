
using CreativeCookies.VideoHosting.DTOs.OAuth;

namespace CreativeCookies.VideoHosting.Contracts.Repositories
{
    public interface IUsersRepository
    {
        Task<UsersPaginatedResultDto> GetUsersList(string search, int pageNumber, int pageSize, string role);
    }
}
