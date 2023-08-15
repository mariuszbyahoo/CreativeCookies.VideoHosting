using CreativeCookies.VideoHosting.Contracts.Enums;
using CreativeCookies.VideoHosting.Contracts.Repositories.OAuth;
using CreativeCookies.VideoHosting.Contracts.Services.OAuth;
using CreativeCookies.VideoHosting.DTOs.OAuth;

namespace CreativeCookies.VideoHosting.Services
{
    public class OAuthClientService : IOAuthClientService
    {
        private readonly IClientStore _clientStore;

        public OAuthClientService(IClientStore clientStore)
        {
            _clientStore = clientStore;
        }

        public async Task<OAuthClientDto> FindByClientIdAsync(Guid clientId)
        {
            var res = await _clientStore.FindByClientIdAsync(clientId);
            return res;
        }

        public async Task<OAuthErrorResponse?> IsCodeWithVerifierValid(string code_verifier, string code, string client_id)
        {
            var res = await _clientStore.IsCodeWithVerifierValid(code_verifier, code, client_id);
            return res;
        }

        public async Task<bool> IsRedirectUriPresentInDatabase(string redirectUri)
        {
            var res = await _clientStore.IsRedirectUriPresentInDatabase(redirectUri);
            return res;
        }

        public async Task<bool> WasRedirectUriRegisteredToClient(string redirect_uri, string client_id)
        {
            var res = await _clientStore.WasRedirectUriRegisteredToClient(redirect_uri, client_id);
            return res;
        }
    }
}
