using Hangfire.Dashboard;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CreativeCookies.VideoHosting.API.Attributes
{
    public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Extract JWT from cookie
            if (httpContext.Request.Cookies.TryGetValue("stac", out string jwtTokenFromCookie))
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(jwtTokenFromCookie);

                var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Role));
                if (roleClaim == null)
                {
                    roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type.Equals("role", StringComparison.InvariantCultureIgnoreCase));
                }
                if (roleClaim != null && (roleClaim.Value.ToLower() == "admin"))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
