using Microsoft.AspNetCore.Identity;

namespace CreativeCookies.VideoHosting.DAL.DAOs.OAuth
{
    public class MyHubUser : IdentityUser<Guid>
    {
        public string StripeCustomerId { get; set; }
    }
}
