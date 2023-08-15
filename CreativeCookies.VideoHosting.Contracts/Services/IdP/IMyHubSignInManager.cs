using CreativeCookies.VideoHosting.DTOs.OAuth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Services.IdP
{
    public interface IMyHubSignInManager
    {
        Task RefreshSignInAsync(MyHubUserDto user);

        Task SignInAsync(MyHubUserDto user, bool isPersistent, string? authenticationMethod = null);

        Task SignOutAsync();

        Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure);

        Task<ExternalLoginInfo?> GetExternalLoginInfoAsync(string? expectedXsrf = null);

        Task<IEnumerable<AuthenticationSchemes>> GetExternalAuthenticationSchemesAsync();

        Task ForgetTwoFactorClientAsync();

        Task<MyHubUserDto?> GetTwoFactorAuthenticationUserAsync();

        AuthenticationProperties ConfigureExternalAuthenticationProperties(string? provider, string? redirectUrl, string? userId = null);

        Task<SignInResult> TwoFactorAuthenticatorSignInAsync(string code, bool isPersistent, bool rememberClient);

        Task<SignInResult> TwoFactorRecoveryCodeSignInAsync(string recoveryCode);


    }
}
