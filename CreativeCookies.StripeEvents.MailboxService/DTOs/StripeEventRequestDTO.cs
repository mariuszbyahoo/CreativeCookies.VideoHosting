namespace CreativeCookies.StripeEvents.MailboxService.DTOs
{
    public class StripeEventRequestDTO
    {
        public string StripeSignature { get; set; }
        public string JsonRequestBody { get; set; }
    }
}
