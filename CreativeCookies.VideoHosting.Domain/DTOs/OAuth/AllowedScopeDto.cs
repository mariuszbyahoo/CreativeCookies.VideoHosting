using CreativeCookies.VideoHosting.Contracts.DTOs.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Domain.DTOs.OAuth
{
    public class AllowedScopeDto : IAllowedScope
    {
        public Guid Id { get; set; }
        public string Scope { get; set; }
        public Guid OAuthClientId { get; set; }
        public IOAuthClient OAuthClient { get; set; }
    }
}
