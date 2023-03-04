using CreativeCookies.VideoHosting.Contracts.Models;

namespace CreativeCookies.VideoHosting.API.Models
{
    public class Video : IVideo
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public int Duration { get; set; }
    }
}
