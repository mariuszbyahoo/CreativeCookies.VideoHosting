namespace CreativeCookies.VideoHosting.API.DTOs
{
    public class StripeCreateSessionRequestDto
    {
        public string PriceId { get; set; }

        public StripeCreateSessionRequestDto(string priceId)
        {
            PriceId = priceId;
        }
    }
}
