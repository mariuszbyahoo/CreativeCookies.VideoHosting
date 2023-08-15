using CreativeCookies.VideoHosting.Contracts.Enums;
using CreativeCookies.VideoHosting.DTOs.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Services.OAuth
{
    public interface IOAuthClientService
    {
        Task<OAuthClientDto> FindByClientIdAsync(Guid clientId);
        Task<bool> IsRedirectUriPresentInDatabase(string redirectUri);
        Task<OAuthErrorResponse?> IsCodeWithVerifierValid(string code_verifier, string code, string client_id);
        Task<bool> WasRedirectUriRegisteredToClient(string redirect_uri, string client_id);
    }
}
