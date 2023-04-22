using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DAL.DAOs.OAuth
{
    public class AuthorizationCode
    {
        public string Code { get; set; }
        public string ClientId { get; set; }
        public string UserId { get; set; }
        public string RedirectUri { get; set; }
        public string CodeChallenge { get; set; }
        public string CodeChallengeMethod { get; set; }
        public DateTime Expiration { get; set; }
    }
}
