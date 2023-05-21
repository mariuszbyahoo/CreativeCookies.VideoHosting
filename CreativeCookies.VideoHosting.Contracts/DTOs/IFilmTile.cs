using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.DTOs
{
    public interface IFilmTile
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ThumbnailName { get; set; }
        public string Length { get; set; }
        public string CreatedOn { get; set; }
        public string BlobUrl { get; set; }
    }
}
