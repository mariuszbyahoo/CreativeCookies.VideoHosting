using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DTOs.OAuth
{
    public class AllowedScopeDto
    {
        public Guid Id { get; set; }
        public string Scope { get; set; }
        public Guid OAuthClientId { get; set; }

        public AllowedScopeDto(Guid id, string scope, Guid oAuthClientId)
        {
            Id = id;
            Scope = scope;
            OAuthClientId = oAuthClientId;
        }
    }
}
