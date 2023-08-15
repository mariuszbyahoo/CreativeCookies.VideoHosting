using CreativeCookies.VideoHosting.Contracts.Repositories.OAuth;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Services
{
    public class AccessTokenService : IAccessTokenService
    {
        private readonly IJWTRepository _jwtRepository;

        public AccessTokenService(IJWTRepository jwtRepository)
        {
            _jwtRepository = jwtRepository;
        }

        public string GetNewAccessToken(Guid userId, string userEmail, Guid clientId, IConfiguration configuration, string issuer, string userRole)
        {
            var res = _jwtRepository.GenerateAccessToken(userId, userEmail, clientId, configuration, issuer, userRole);
            return res;
        }
    }
}
