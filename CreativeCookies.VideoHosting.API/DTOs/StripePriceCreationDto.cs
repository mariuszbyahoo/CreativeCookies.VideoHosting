namespace CreativeCookies.VideoHosting.API.DTOs
{
    public class StripePriceCreationDto
    {
        public string StripeProductId { get; set; }
        public string CurrencyCode { get; set; }
        public int UnitAmount { get; set; }
    }
}
