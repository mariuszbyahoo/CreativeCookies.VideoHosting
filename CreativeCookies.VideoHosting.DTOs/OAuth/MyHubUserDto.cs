
namespace CreativeCookies.VideoHosting.DTOs.OAuth
{
    public class MyHubUserDto
    {
        public Guid Id { get; set; }
        public string UserEmail { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public string StripeCustomerId { get; set; }
        public MyHubUserDto(Guid id, string userEmail, string role, bool isActive, string stripeCustomerId)
        {
            Id = id;
            UserEmail = userEmail;
            Role = role;
            IsActive = isActive;
            StripeCustomerId = stripeCustomerId;
        }
    }
}
