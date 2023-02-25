namespace CreativeCookies.VideoHosting.Contracts
{
    public interface IVideo
    {
        int Id { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        string Location { get; set; }
        int Duration { get; set; }
        DateTime UploadDate { get; set; }
    }
}