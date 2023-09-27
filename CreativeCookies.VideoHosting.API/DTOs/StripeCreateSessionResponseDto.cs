namespace CreativeCookies.VideoHosting.API.DTOs
{
    public class StripeCreateSessionResponseDto
    {
        public string DestinationUrl { get; set; }

        public StripeCreateSessionResponseDto(string destinationUrl)
        {
            DestinationUrl = destinationUrl;
        }
    }
}
