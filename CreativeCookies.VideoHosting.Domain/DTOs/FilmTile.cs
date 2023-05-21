using CreativeCookies.VideoHosting.Contracts.DTOs;

namespace CreativeCookies.VideoHosting.Domain.DTOs
{
    public class FilmTile : IFilmTile
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ThumbnailName { get; set; }
        public string Length { get; set; }
        public string CreatedOn { get; set; }
        public string BlobUrl { get; set; }
    }
}