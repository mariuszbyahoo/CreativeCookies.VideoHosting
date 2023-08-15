using CreativeCookies.VideoHosting.Contracts.Enums;
using CreativeCookies.VideoHosting.Contracts.Repositories.OAuth;
using CreativeCookies.VideoHosting.DAL.Contexts;
using CreativeCookies.VideoHosting.DTOs.OAuth;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace CreativeCookies.VideoHosting.DAL.Repositories.OAuth
{
    public class ClientStore : IClientStore
    {
        private readonly AppDbContext _ctx;

        public ClientStore(AppDbContext ctx)
        {
            _ctx = ctx;
        }
        public async Task<OAuthClientDto> FindByClientIdAsync(Guid clientId)
        {
            var client = await _ctx.OAuthClients
                        .Include(c => c.AllowedScopes)
                        .FirstOrDefaultAsync(c => c.Id.Equals(clientId));
            if (client == null)
            {
                return null;
            }
            else
            {
                var clientDto = new OAuthClientDto(client.Id, client.ClientSecret, client.RedirectUri,
                    client.AllowedScopes.Select(scope => new AllowedScopeDto(scope.Id, scope.Scope, scope.OAuthClientId)).ToList());
                return clientDto;
            }
        }
        public async Task<bool> IsRedirectUriPresentInDatabase(string redirectUri)
        {
            var client = await _ctx.OAuthClients.FirstOrDefaultAsync(c => c.RedirectUri.Equals(redirectUri));
            if (client == null) return false;
            else return true;
        }

        public async Task<OAuthErrorResponse?> IsCodeWithVerifierValid(string code_verifier, string code, string client_id)
        {
            var entry = await _ctx.AuthorizationCodes.Where(c => c.Code.Equals(code)).FirstOrDefaultAsync();
            if (string.IsNullOrWhiteSpace(code_verifier)) return OAuthErrorResponse.InvalidRequest;
            if (entry == null) return OAuthErrorResponse.InvalidRequest;
            if (!entry.ClientId.Equals(client_id)) return OAuthErrorResponse.InvalidRequest;
            if (entry.Expiration < DateTime.UtcNow) return OAuthErrorResponse.InvalidGrant;

            if (entry.CodeChallengeMethod.ToLower().Equals("s256"))
            {
                using var sha256 = SHA256.Create();
                var codeVerifierBytes = Encoding.UTF8.GetBytes(code_verifier);
                var hashedBytes = sha256.ComputeHash(codeVerifierBytes);
                var hashedBase64Url = Convert.ToBase64String(hashedBytes).Replace('+', '-').Replace('/', '_').Replace("=", "");

                if (!hashedBase64Url.Equals(entry.CodeChallenge))
                {
                    return OAuthErrorResponse.InvalidGrant;
                }
            } // plain method not supported
            else
            {
                return OAuthErrorResponse.InvalidRequest;
            }

            return null;
        }

        public async Task<bool> WasRedirectUriRegisteredToClient(string redirect_uri, string client_id)
        {
            var entry = await _ctx.OAuthClients.FirstOrDefaultAsync(c => c.Id.ToString().Equals(client_id));
            if (entry == null) return false;
            else if (entry.RedirectUri.Equals(redirect_uri)) return true;
            else return false;
        }
    }
}
