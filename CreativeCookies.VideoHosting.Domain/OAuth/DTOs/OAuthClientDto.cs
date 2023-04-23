using CreativeCookies.VideoHosting.Contracts.DTOs.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Domain.OAuth.DTOs
{
    public class OAuthClientDto : IOAuthClient
    {
        public Guid Id { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUri { get; set; }
        public ICollection<IAllowedScope> AllowedScopes { get; set; }
    }
}
