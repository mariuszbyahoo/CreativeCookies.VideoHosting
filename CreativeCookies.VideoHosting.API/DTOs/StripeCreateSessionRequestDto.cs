namespace CreativeCookies.VideoHosting.API.DTOs
{
    public class StripeCreateSessionRequestDto
    {
        public string PriceId { get; set; }
        public bool IsCoolingOffPeriodApplicable { get; set; }

        public StripeCreateSessionRequestDto(string priceId, bool isCoolingOffPeriodApplicable = false)
        {
            PriceId = priceId;
            IsCoolingOffPeriodApplicable = isCoolingOffPeriodApplicable;
        }
    }
}
