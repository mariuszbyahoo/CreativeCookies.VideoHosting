namespace CreativeCookies.VideoHosting.API.DTOs
{
    public class MyHubUserDownloadDTO
    {
        public string UserEmail { get; set; }
        public string Role { get; set; }
        public string IsUserActive { get; set; }
        public string InvoicePeriodStartDateUTC { get; set; }
        public string InvoicePeriodEndDateUTC { get; set; }
        public string StripeCustomerId { get; set; }

        public MyHubUserDownloadDTO(string userEmail, string role, string isUserActive, string invoicePeriodStartDateUTC, string invoicePeriodEndDateUTC, string stripeCustomerId)
        {
            UserEmail = userEmail;
            Role = role;
            IsUserActive = isUserActive;
            InvoicePeriodStartDateUTC = invoicePeriodStartDateUTC;
            InvoicePeriodEndDateUTC = invoicePeriodEndDateUTC;
            StripeCustomerId = stripeCustomerId;
        }
    }
}
