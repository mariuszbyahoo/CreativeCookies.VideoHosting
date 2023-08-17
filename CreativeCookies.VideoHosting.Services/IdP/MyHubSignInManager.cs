using CreativeCookies.VideoHosting.Contracts.Services.IdP;
using CreativeCookies.VideoHosting.DAL.DAOs.OAuth;
using CreativeCookies.VideoHosting.DTOs.OAuth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace CreativeCookies.VideoHosting.Services.IdP
{
    public class MyHubSignInManager : IMyHubSignInManager
    {
        private readonly SignInManager<MyHubUser> _signInManager;

        public MyHubSignInManager(SignInManager<MyHubUser> usrMgr)
        {
            _signInManager = usrMgr;
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
            var dto = new MyHubUserDto(Guid.Parse(res.Id), res.Email, "", res.EmailConfirmed, res.StripeCustomerId);
            return dto;
        }

        public async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            var res = await _signInManager.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);
            return res;
        }

        public async Task RefreshSignInAsync(MyHubUserDto user)
        {
            // HACK: Niezgodność typów KU**A! najpierw dostosuj DAL do MyHubUser a potem się z tym baw.
            throw new NotImplementedException();
        }

        public Task SignInAsync(MyHubUserDto user, bool isPersistent, string? authenticationMethod = null)
        {
            throw new NotImplementedException();
        }

        public Task SignOutAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Microsoft.AspNetCore.Identity.SignInResult> TwoFactorAuthenticatorSignInAsync(string code, bool isPersistent, bool rememberClient)
        {
            throw new NotImplementedException();
        }

        public Task<Microsoft.AspNetCore.Identity.SignInResult> TwoFactorRecoveryCodeSignInAsync(string recoveryCode)
        {
            throw new NotImplementedException();
        }
    }
}
