
using CreativeCookies.VideoHosting.DTOs.OAuth;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace CreativeCookies.VideoHosting.Contracts.Services.IdP
{
    /// <summary>
    /// Interface serves as a wrapper around UserManager
    /// Hides the DAL references from API.
    /// </summary>
    public interface IMyHubUserManager
    {
        Task<IdentityResult> CreateAsync(MyHubUserDto user, string password);

        Task<IdentityResult> AddToRoleAsync(MyHubUserDto user, string role);

        Task<IList<string>> GetRolesAsync(MyHubUserDto user);

        string? GetUserId(ClaimsPrincipal principal);

        Task<MyHubUserDto?> FindByIdAsync(string id);

        Task<MyHubUserDto> GetUserAsync(ClaimsPrincipal principal);

        Task<IdentityResult> SetUserNameAsync(MyHubUserDto user, string userName);

        #region email

        Task<MyHubUserDto?> FindByEmailAsync(string email);

        /// <summary>
        /// Getter for UserManager's property
        /// </summary>
        /// <returns>SupportsUserEmail propperty</returns>
        bool GetManagerSupportsUserEmail();

        Task<string> GenerateChangeEmailTokenAsync(MyHubUserDto user, string newEmail);

        Task<string> GenerateEmailConfirmationTokenAsync(MyHubUserDto user);

        Task<string?> GetEmailAsync(MyHubUserDto user);

        Task<IdentityResult> ConfirmEmailAsync(MyHubUserDto user, string token);

        Task<IdentityResult> ChangeEmailAsync(MyHubUserDto user, string newEmail, string token);

        Task<bool> IsEmailConfirmedAsync(MyHubUserDto user);

        #endregion

        #region password

        Task<string> GeneratePasswordResetTokenAsync(MyHubUserDto user);

        Task<bool> HasPasswordAsync(MyHubUserDto user);

        Task<IdentityResult> ChangePasswordAsync(MyHubUserDto user, string currentPassword, string newPassword);

        Task<bool> CheckPasswordAsync(MyHubUserDto user, string password);

        #endregion

        #region twoFactor

        Task<int> CountRecoveryCodesAsync(MyHubUserDto user);

        Task<IdentityResult> ResetAuthenticatorKeyAsync(MyHubUserDto user);

        /// <summary>
        /// Below method encapsulates an UserManager's field
        /// </summary>
        /// <returns>UserManager.Options.Tokens.AuthenticatorTokenProvider value</returns>
        string GetAuthenticatorTokenProvider();

        Task<string?> GetAuthenticatorKeyAsync(MyHubUserDto user);

        Task<bool> VerifyTwoFactorTokenAsync(MyHubUserDto user, string tokenProvider, string token);

        Task<bool> GetTwoFactorEnabledAsync(MyHubUserDto user);

        Task<IEnumerable<string>?> GenerateNewTwoFactorRecoveryCodesAsync(MyHubUserDto user, int number);

        Task<IdentityResult> SetTwoFactorEnabledAsync(MyHubUserDto user, bool enabled);

        #endregion

        Task<IdentityResult> DeleteAsync(MyHubUserDto user);

        Task<IdentityResult> RemoveLoginAsync(MyHubUserDto user, string loginProvider, string providerKey);

        Task<IList<UserLoginInfo>> GetLoginsAsync(MyHubUserDto user);

        Task<string?> GetPhoneNumberAsync(MyHubUserDto user);

        Task<IdentityResult> SetPhoneNumberAsync(MyHubUserDto user, string phoneNumber);
        
        Task<string> GetUserIdAsync(MyHubUserDto user);

        Task<string> GetUserNameAsync(MyHubUserDto user);

        Task<IdentityResult> AddLoginAsync(MyHubUserDto user, ExternalLoginInfo externalLoginInfo);

        Task<IdentityResult> ResetPasswordAsync(MyHubUserDto user, string token, string newPassword);

        string? GetUserName(ClaimsPrincipal claimsPrincipal);
    }
}
