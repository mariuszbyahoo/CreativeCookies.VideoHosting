using CreativeCookies.VideoHosting.Contracts.Enums;
using CreativeCookies.VideoHosting.DTOs.OAuth;

namespace CreativeCookies.VideoHosting.Contracts.Repositories.OAuth
{
    public interface IClientStore
    {
        Task<OAuthClientDto> FindByClientIdAsync(Guid clientId);
        Task<bool> IsRedirectUriPresentInDatabase(string redirectUri);
        Task<OAuthErrorResponse?> IsCodeWithVerifierValid(string code_verifier, string code, string client_id);
        Task<bool> WasRedirectUriRegisteredToClient(string redirect_uri, string client_id);
    }
}
