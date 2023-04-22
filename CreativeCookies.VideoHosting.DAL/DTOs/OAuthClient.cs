using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DAL.DTOs
{
    public class OAuthClient
    {
        public Guid Id { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUri { get; set; }
        public ICollection<string> AllowedScopes { get; set; }
    }
}
