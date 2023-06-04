using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DAL.DAOs.OAuth
{
    public class RefreshTokenDAO
    {
        public Guid Id { get; set; }
        public string Token { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime Expires { get; set; }
        public bool IsRevoked { get; set; }
        public string UserId { get; set; } 
    }
}
