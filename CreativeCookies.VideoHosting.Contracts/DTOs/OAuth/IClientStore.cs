using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.DTOs.OAuth
{
    public interface IClientStore
    {
        Task<IOAuthClient> FindByClientIdAsync(Guid clientId);
        Task<string> GetAuthorizationCode(string client_id, string userId, string redirect_uri, string code_challenge, string code_challenge_method);
    }
}
