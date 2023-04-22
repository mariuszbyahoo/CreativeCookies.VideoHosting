using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.DTOs.OAuth
{
    public interface IOAuthClient
    {
        Guid Id { get; set; }
        string ClientId { get; set; }
        string ClientSecret { get; set; }
        string RedirectUri { get; set; }
        ICollection<IAllowedScope> AllowedScopes { get; set; }
    }
}
