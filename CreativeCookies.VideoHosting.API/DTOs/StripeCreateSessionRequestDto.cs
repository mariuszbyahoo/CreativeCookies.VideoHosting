namespace CreativeCookies.VideoHosting.API.DTOs
{
    public class StripeCreateSessionRequestDto
    {
        public string PriceId { get; set; }

        public StripeCreateSessionRequestDto(string priceId)
        {
            PriceId = priceId;
            // HACK Task 178: Add non obligatory field: isCoolingOffPeriodApplicable - EU's 14 days of cooling off period.
        }
    }
}
