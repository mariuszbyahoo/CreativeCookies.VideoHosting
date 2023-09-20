using Microsoft.AspNetCore.Identity;
using System.Runtime.CompilerServices;

namespace CreativeCookies.VideoHosting.DAL.DAOs.OAuth
{
    public class MyHubUser : IdentityUser
    {
        public string StripeCustomerId { get; set; }
        public DateTime SubscriptionEndDateUTC { get; set; }
    }
}
