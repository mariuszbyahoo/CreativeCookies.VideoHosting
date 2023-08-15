using CreativeCookies.VideoHosting.Contracts.Repositories.OAuth;
using CreativeCookies.VideoHosting.Contracts.Services.OAuth;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Services.OAuth
{
    public class AccessTokenService : IAccessTokenService
    {
        private readonly IJWTGenerator _jwtGenerator;

        public AccessTokenService(IJWTGenerator jwtRepository)
        {
            _jwtGenerator = jwtRepository;
        }

        public string GetNewAccessToken(Guid userId, string userEmail, Guid clientId, IConfiguration configuration, string issuer, string userRole)
        {
            var res = _jwtGenerator.GenerateAccessToken(userId, userEmail, clientId, configuration, issuer, userRole);
            return res;
        }
    }
}
