using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Repositories.OAuth
{
    public interface IJWTRepository
    {
        string GenerateAccessToken(Guid userId, string userEmail, Guid clientId, IConfiguration configuration, string issuer);
    }
}
