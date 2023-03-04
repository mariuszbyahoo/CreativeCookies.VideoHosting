namespace CreativeCookies.VideoHosting.Contracts.Models
{
    public interface IVideo
    {
        Guid Id { get; }
        string Name { get; set; }
        string Description { get; set; }
        string Location { get; set; }
        int Duration { get; set; }
    }
}