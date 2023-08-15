using CreativeCookies.VideoHosting.Contracts.Repositories;
using CreativeCookies.VideoHosting.Contracts.Services;
using CreativeCookies.VideoHosting.DTOs.OAuth;

namespace CreativeCookies.VideoHosting.Services
{
    public class UsersService : IUsersService
    {
        private readonly IUsersRepository _repo;

        public UsersService(IUsersRepository repo)
        {
            _repo = repo;
        }

        public async Task<UsersPaginatedResultDto> GetUsersPaginatedResult(string search, int pageNumber, int pageSize, string role)
        {
            var paginatedResult = await _repo.GetUsersPaginatedResult(search, pageNumber, pageSize, role);
            return paginatedResult;
        }
    }
}
