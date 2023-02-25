namespace CreativeCookies.VideoHosting.Contracts
{
    public interface IVideo
    {
        Guid Id { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        string Location { get; set; }
        int Duration { get; set; }
    }
}