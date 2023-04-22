namespace CreativeCookies.VideoHosting.Contracts.DTOs.OAuth
{
    public interface IAllowedScope
    {
        Guid Id { get; set; }
        string Scope { get; set; }
        Guid OAuthClientId { get; set; }
        IOAuthClient OAuthClient { get; set; }
    }
}