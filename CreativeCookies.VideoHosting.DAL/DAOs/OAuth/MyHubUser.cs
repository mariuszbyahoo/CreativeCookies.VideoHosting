using Microsoft.AspNetCore.Identity;
using System.Runtime.CompilerServices;

namespace CreativeCookies.VideoHosting.DAL.DAOs.OAuth
{
    public class MyHubUser : IdentityUser
    {
        public string StripeCustomerId { get; set; }
        public DateTime SubscriptionStartDateUTC { get; set; }
        public DateTime SubscriptionEndDateUTC { get; set; }
        /// <summary>
        /// Id of Hangfire Job, which will create a subscription in the background after 14 days cooling off period
        /// </summary>
        public string HangfireJobId { get; set; }
    }
}
