namespace CreativeCookies.VideoHosting.API.DTOs
{
    public class StripeCreateSessionRequestDto
    {
        public string PriceId { get; set; }
        public bool HasDeclinedCoolingOffPeriod { get; set; }

        public StripeCreateSessionRequestDto(string priceId, bool HasDeclinedCoolingOffPeriod = false)
        {
            PriceId = priceId;
            this.HasDeclinedCoolingOffPeriod = HasDeclinedCoolingOffPeriod;
        }
    }
}
