using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Domain.OAuth
{
    public static class AuthCodeGenerator
    {
        /// <summary>
        /// Generates an Authorization code
        /// </summary>
        /// <returns>URL safe Base-64 encoded Authorization Code</returns>
        public static string GenerateAuthorizationCode()
        {
            int codeLength = 32; 
            byte[] randomBytes = new byte[codeLength];

            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            string code = Convert.ToBase64String(randomBytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');

            return code;
        }
    }
}
