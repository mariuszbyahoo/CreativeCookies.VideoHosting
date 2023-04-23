using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DAL.DAOs.OAuth
{
    public class OAuthClient 
    {
        public Guid Id { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUri { get; set; }
        public ICollection<AllowedScope> AllowedScopes { get; set; }
    }
}
