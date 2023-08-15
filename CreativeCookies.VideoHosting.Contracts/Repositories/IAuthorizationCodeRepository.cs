
using CreativeCookies.VideoHosting.DTOs.OAuth;

namespace CreativeCookies.VideoHosting.Contracts.Repositories
{
    public interface IAuthorizationCodeRepository
    {
        /// <summary>
        /// Saves an AuthorizationCode for particular user and particular client_id to the database 
        /// </summary>
        /// <param name="client_id"></param>
        /// <param name="userId"></param>
        /// <param name="redirect_uri"></param>
        /// <param name="code_challenge"></param>
        /// <param name="code_challenge_method"></param>
        /// <param name="authorizationCode"></param>
        /// <returns>Saved Authorization Code</returns>
        Task SaveAuthorizationCode(string client_id, string userId, string redirect_uri, string code_challenge, string code_challenge_method, string authorizationCode);
        
        /// <summary>
        /// Clears expired AuthCodes
        /// </summary>
        /// <returns></returns>
        Task ClearExpiredAuthorizationCodes();

        /// <summary>
        /// Gets a user by an AuthCode
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        Task<MyHubUserDto> GetUserByAuthCodeAsync(string code);

        /// <summary>
        /// Deletes previous authCodes issued for particular user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="client_id"></param>
        /// <returns></returns>
        Task DeletePreviousAuthCodesForUser(string userId, string client_id);
    }
}
