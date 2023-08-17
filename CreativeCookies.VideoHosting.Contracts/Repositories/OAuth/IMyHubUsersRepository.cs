using CreativeCookies.VideoHosting.DTOs.OAuth;

namespace CreativeCookies.VideoHosting.Contracts.Repositories.OAuth
{
    public interface IMyHubUsersRepository
    {
        /// <summary>
        /// Looks up a user with supplied ID
        /// </summary>
        /// <param name="id">Id to look for in database</param>
        /// <returns>An entity mapped to MyHubUserDto</returns>
        Task<MyHubUserDto> GetUser(Guid id);

        /// <summary>
        /// Looks up a user with supplied Email 
        /// </summary>
        /// <param name="email">email address to look for in database</param>
        /// <returns>An entity mapped to MyHubUserDto</returns>
        Task<MyHubUserDto> GetUser(string email);

        /// <summary>
        /// Looks up a set of users with a role supplied in an argument
        /// </summary>
        /// <param name="role">Role to look for in database</param>
        /// <returns>An entities mapped to IEnumerable with MyHubUserDtos</returns>
        Task<MyHubUserDto> GetUsers(string role);

        /// <summary>
        /// Gets all MyHub's users
        /// </summary>
        /// <returns>An entities mapped to IEnumerable with MyHubUserDtos</returns>
        Task<MyHubUserDto> GetUsers();
    }
}
