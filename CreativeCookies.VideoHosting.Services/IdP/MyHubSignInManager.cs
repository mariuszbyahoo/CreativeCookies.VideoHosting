using CreativeCookies.VideoHosting.Contracts.Services.IdP;
using CreativeCookies.VideoHosting.DAL.DAOs.OAuth;
using CreativeCookies.VideoHosting.DTOs.OAuth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace CreativeCookies.VideoHosting.Services.IdP
{
    public class MyHubSignInManager : IMyHubSignInManager
    {
        private readonly SignInManager<MyHubUser> _signInManager;
        private readonly UserManager<MyHubUser> _userManager;

        public MyHubSignInManager(SignInManager<MyHubUser> signInManager, UserManager<MyHubUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public AuthenticationProperties ConfigureExternalAuthenticationProperties(string? provider, string? redirectUrl, string? userId = null)
        {
            var res = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, userId);
            return res;
        }

        public async Task ForgetTwoFactorClientAsync()
        {
            await _signInManager.ForgetTwoFactorClientAsync();
        }

        public async Task<IEnumerable<AuthenticationScheme>> GetExternalAuthenticationSchemesAsync()
        {
            var res = await _signInManager.GetExternalAuthenticationSchemesAsync();
            return res;
        }

        public async Task<ExternalLoginInfo?> GetExternalLoginInfoAsync(string? expectedXsrf = null)
        {
            var res = await _signInManager.GetExternalLoginInfoAsync();
            return res;
        }

        public async Task<MyHubUserDto?> GetTwoFactorAuthenticationUserAsync()
        {
            var res = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (res == null) return null;
            var dto = new MyHubUserDto(Guid.Parse(res.Id), res.Email, "", res.EmailConfirmed, res.StripeCustomerId, res.SubscriptionStartDateUTC, res.SubscriptionEndDateUTC, res.HangfireJobId);
            return dto;
        }

        public async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            var res = await _signInManager.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);
            return res;
        }

        public async Task RefreshSignInAsync(MyHubUserDto user)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            if(dao != null) await _signInManager.RefreshSignInAsync(dao);
        }

        public async Task SignInAsync(MyHubUserDto user, bool isPersistent, string? authenticationMethod = null)
        {
            var dao = await _userManager.FindByIdAsync(user.Id.ToString());
            if (dao != null) await _signInManager.SignInAsync(dao, isPersistent, authenticationMethod);
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<SignInResult> TwoFactorAuthenticatorSignInAsync(string code, bool isPersistent, bool rememberClient)
        {
            return await _signInManager.TwoFactorAuthenticatorSignInAsync(code, isPersistent, rememberClient);
        }

        public async Task<SignInResult> TwoFactorRecoveryCodeSignInAsync(string recoveryCode)
        {
            return await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);
        }

        public async Task<bool> IsTwoFactorClientRememberedAsync(MyHubUserDto dto)
        {
            var dao = await _userManager.FindByIdAsync(dto.Id.ToString());
            return await _signInManager.IsTwoFactorClientRememberedAsync(dao);
        }

        public bool IsSignedIn(ClaimsPrincipal claimsPrincipal)
        {
            return _signInManager.IsSignedIn(claimsPrincipal);
        }
    }
}
