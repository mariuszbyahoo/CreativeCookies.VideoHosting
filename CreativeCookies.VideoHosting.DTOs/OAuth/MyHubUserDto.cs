
namespace CreativeCookies.VideoHosting.DTOs.OAuth
{
    public class MyHubUserDto
    {
        public Guid Id { get; set; }
        public string UserEmail { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public string StripeCustomerId { get; set; }
        public DateTime SubscriptionStartDateUTC { get; set; }
        public DateTime SubscriptionEndDateUTC { get; set; }
        public MyHubUserDto(Guid id, string userEmail, string role, bool isActive, string stripeCustomerId, DateTime subscriptionStartDateUTC, DateTime subscriptionEndDateUTC)
        {
            Id = id;
            UserEmail = userEmail;
            Role = role;
            IsActive = isActive;
            StripeCustomerId = stripeCustomerId;
            SubscriptionStartDateUTC = subscriptionStartDateUTC;
            SubscriptionEndDateUTC = subscriptionEndDateUTC;
        }
    }
}
