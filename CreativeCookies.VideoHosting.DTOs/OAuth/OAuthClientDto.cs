using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DTOs.OAuth
{
    public class OAuthClientDto
    {
        public Guid Id { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUri { get; set; }
        public ICollection<AllowedScopeDto> AllowedScopes { get; set; }

        public OAuthClientDto(Guid id, string clientSecret, string redirectUri, ICollection<AllowedScopeDto> allowedScopes)
        {
            Id = id;
            ClientSecret = clientSecret;
            RedirectUri = redirectUri;
            AllowedScopes = allowedScopes;
        }
    }
}
