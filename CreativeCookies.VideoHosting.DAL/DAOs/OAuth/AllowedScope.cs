using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DAL.DAOs.OAuth
{
    public class AllowedScope 
    {
        public Guid Id { get; set; }
        public string Scope { get; set; }
        public Guid OAuthClientId { get; set; }
        public OAuthClient OAuthClient { get; set; }
    }

}
