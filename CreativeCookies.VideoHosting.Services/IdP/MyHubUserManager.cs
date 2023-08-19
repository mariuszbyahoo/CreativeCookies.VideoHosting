
using CreativeCookies.VideoHosting.Contracts.Services.IdP;
using CreativeCookies.VideoHosting.DAL.DAOs.OAuth;
using CreativeCookies.VideoHosting.DTOs.OAuth;
using Microsoft.AspNetCore.Identity;
using System.Data;
using System.Security.Claims;

namespace CreativeCookies.VideoHosting.Services.IdP
{
    public class MyHubUserManager : IMyHubUserManager
    {
        private readonly UserManager<MyHubUser> _userManager;

        public MyHubUserManager(UserManager<MyHubUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IdentityResult> AddToRoleAsync(MyHubUserDto user, string role)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            return await _userManager.AddToRoleAsync(dao, role);
        }

        public async Task<IdentityResult> ChangeEmailAsync(MyHubUserDto user, string newEmail, string token)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            return await _userManager.ChangeEmailAsync(dao, newEmail, token);
        }

        public async Task<IdentityResult> ChangePasswordAsync(MyHubUserDto user, string currentPassword, string newPassword)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            return await _userManager.ChangePasswordAsync(dao, currentPassword, newPassword);
        }

        public async Task<bool> CheckPasswordAsync(MyHubUserDto user, string password)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            return await _userManager.CheckPasswordAsync(dao, password);
        }

        public async Task<IdentityResult> ConfirmEmailAsync(MyHubUserDto user, string token)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            return await _userManager.ConfirmEmailAsync(dao, token);
        }

        public async Task<int> CountRecoveryCodesAsync(MyHubUserDto user)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            return await _userManager.CountRecoveryCodesAsync(dao);
        }

        public async Task<IdentityResult> CreateAsync(MyHubUserDto user, string password)
        {
            var dao = new MyHubUser() { Email = user.UserEmail, UserName = user.UserEmail };
            return await _userManager.CreateAsync(dao, password);
        }

        public async Task<IdentityResult> DeleteAsync(MyHubUserDto user)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            return await _userManager.DeleteAsync(dao);
        }

        public async Task<MyHubUserDto?> FindByEmailAsync(string email)
        {
            var dao = await _userManager.FindByEmailAsync(email);
            var roles = string.Join(',', await _userManager.GetRolesAsync(dao));
            return new MyHubUserDto(Guid.Parse(dao.Id), dao.Email, roles, dao.EmailConfirmed, dao.StripeCustomerId);
        }

        public async Task<MyHubUserDto?> FindByIdAsync(string id)
        {
            var dao = await _userManager.FindByIdAsync(id);
            var roles = string.Join(',', await _userManager.GetRolesAsync(dao));
            return new MyHubUserDto(Guid.Parse(dao.Id), dao.Email, roles, dao.EmailConfirmed, dao.StripeCustomerId);
        }

        public async Task<string> GenerateChangeEmailTokenAsync(MyHubUserDto user, string newEmail)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            return await _userManager.GenerateChangeEmailTokenAsync(dao, newEmail);
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(MyHubUserDto user)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            return await _userManager.GenerateEmailConfirmationTokenAsync(dao);
        }

        public async Task<IEnumerable<string>?> GenerateNewTwoFactorRecoveryCodesAsync(MyHubUserDto user, int number)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            return await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(dao, number);
        }

        public async Task<string> GeneratePasswordResetTokenAsync(MyHubUserDto user)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            return await _userManager.GeneratePasswordResetTokenAsync(dao);
        }

        public async Task<string?> GetAuthenticatorKeyAsync(MyHubUserDto user)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            return await _userManager.GetAuthenticatorKeyAsync(dao);
        }

        public string GetAuthenticatorTokenProvider()
        {
            return _userManager.Options.Tokens.AuthenticatorTokenProvider;
        }

        public async Task<string?> GetEmailAsync(MyHubUserDto user)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            return await _userManager.GetEmailAsync(dao);
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(MyHubUserDto user)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            return await _userManager.GetLoginsAsync(dao);
        }

        public bool GetManagerSupportsUserEmail()
        {
            return _userManager.SupportsUserEmail;
        }

        public async Task<string?> GetPhoneNumberAsync(MyHubUserDto user)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            return await _userManager.GetPhoneNumberAsync(dao);
        }

        public async Task<IList<string>> GetRolesAsync(MyHubUserDto user)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            return await _userManager.GetRolesAsync(dao);
        }

        public async Task<bool> GetTwoFactorEnabledAsync(MyHubUserDto user)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            return await _userManager.GetTwoFactorEnabledAsync(dao);
        }

        public async Task<MyHubUserDto> GetUserAsync(ClaimsPrincipal principal)
        {
            var dao = await _userManager.GetUserAsync(principal);
            var roles = string.Join(',', await _userManager.GetRolesAsync(dao));
            return new MyHubUserDto(Guid.Parse(dao.Id), dao.Email, roles, dao.EmailConfirmed, dao.StripeCustomerId);
        }

        public string? GetUserId(ClaimsPrincipal principal)
        {
            return _userManager.GetUserId(principal);

        }

        public async Task<string> GetUserIdAsync(MyHubUserDto user)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            return await _userManager.GetUserIdAsync(dao);
        }

        public async Task<bool> HasPasswordAsync(MyHubUserDto user)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            return await _userManager.HasPasswordAsync(dao);
        }

        public async Task<bool> IsEmailConfirmedAsync(MyHubUserDto user)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            return await _userManager.IsEmailConfirmedAsync(dao);
        }

        public async Task<IdentityResult> RemoveLoginAsync(MyHubUserDto user, string loginProvider, string providerKey)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            return await _userManager.RemoveLoginAsync(dao, loginProvider, providerKey);
        }

        public async Task<IdentityResult> ResetAuthenticatorKeyAsync(MyHubUserDto user)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            return await _userManager.ResetAuthenticatorKeyAsync(dao);
        }

        public async Task<IdentityResult> SetPhoneNumberAsync(MyHubUserDto user, string phoneNumber)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            return await _userManager.SetPhoneNumberAsync(dao, phoneNumber);
        }

        public async Task<IdentityResult> SetTwoFactorEnabledAsync(MyHubUserDto user, bool enabled)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            return await _userManager.SetTwoFactorEnabledAsync(dao, enabled);
        }

        public async Task<IdentityResult> SetUserNameAsync(MyHubUserDto user, string userName)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            return await _userManager.SetUserNameAsync(dao, userName);
        }

        public async Task<bool> VerifyTwoFactorTokenAsync(MyHubUserDto user, string tokenProvider, string token)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            return await _userManager.VerifyTwoFactorTokenAsync(dao, tokenProvider, token);
        }

        public async Task<IdentityResult> AddLoginAsync(MyHubUserDto user, ExternalLoginInfo externalLoginInfo)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            return await _userManager.AddLoginAsync(dao, externalLoginInfo);
        }

        public async Task<string> GetUserNameAsync(MyHubUserDto user)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            return await _userManager.GetUserNameAsync(dao);
        }

        public async Task<IdentityResult> ResetPasswordAsync(MyHubUserDto user, string token, string newPassword)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            return await _userManager.ResetPasswordAsync(dao, token, newPassword);
        }

        public string? GetUserName(ClaimsPrincipal claimsPrincipal)
        {
            return _userManager.GetUserName(claimsPrincipal);
        }

        public async Task<string> GetPasswordHashAsync(MyHubUserDto user)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            return dao.PasswordHash;
        }

        public async Task<Dictionary<string, string>> GetPersonalDataDictionaryToDownload(MyHubUserDto user)
        {
            var res = new Dictionary<string, string>();
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());

            var personalDataProps = typeof(MyHubUser).GetProperties().Where(
                prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
            foreach (var p in personalDataProps)
            {
                res.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
            }

            var logins = await GetLoginsAsync(user);
            foreach (var l in logins)
            {
                res.Add($"{l.LoginProvider} external login provider key", l.ProviderKey);
            }

            res.Add($"Authenticator Key", await _userManager.GetAuthenticatorKeyAsync(dao));

            return res;
        }
    }
}
