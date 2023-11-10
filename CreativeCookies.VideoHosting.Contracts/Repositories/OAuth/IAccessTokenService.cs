using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Repositories.OAuth
{
    public interface IAccessTokenService
    {
        public string GetNewAccessToken(Guid userId, string userEmail, Guid clientId, IConfiguration configuration, string issuer, string userRole);

        public string GetUserIdFromToken(string accessToken);
    }
}
