using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Enums
{
    public enum OAuthErrorResponse
    {
        /// <summary>
        /// In this case should return message with error: "invalid_request"
        /// </summary>
        InvalidRequest = 0,
        /// <summary>
        /// In this case should NOT return anything, just return BadRequest
        /// </summary>
        InvalidRedirectUri = 1,
        /// <summary>
        /// In this case should return message with error "unauthorised_client"
        /// </summary>
        UnauthorisedClient = 2,
        /// <summary>
        /// In this case should return message with error "invalid_grant"
        /// </summary>
        InvalidGrant = 3,
    }
}
