using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CreativeCookies.VideoHosting.Contracts.DTOs.OAuth;
using CreativeCookies.VideoHosting.Contracts.Enums;

namespace CreativeCookies.VideoHosting.Contracts.Repositories.OAuth
{
    public interface IClientStore
    {
        Task<IOAuthClient> FindByClientIdAsync(Guid clientId);
        Task<bool> IsRedirectUriPresentInDatabase(string redirectUri);
        Task<OAuthErrorResponse?> IsCodeWithVerifierValid(string code_verifier, string code, string client_id);
        Task<bool> WasRedirectUriRegisteredToClient(string redirect_uri, string client_id);
    }
}
