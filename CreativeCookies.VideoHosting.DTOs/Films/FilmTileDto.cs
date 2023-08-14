using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.DTOs.Films
{
    public class FilmTileDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ThumbnailName { get; set; }
        public string Length { get; set; }
        public string CreatedOn { get; set; }
        public string BlobUrl { get; set; }

        public FilmTileDto(Guid id, string name, string description, string thumbnailName, string length, string createdOn, string blobUrl)
        {
            Id = id;
            Name = name;
            Description = description;
            ThumbnailName = thumbnailName;
            Length = length;
            CreatedOn = createdOn;
            BlobUrl = blobUrl;
        }
    }
}
