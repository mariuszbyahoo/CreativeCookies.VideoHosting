using CreativeCookies.VideoHosting.Contracts.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Domain.DTOs
{
    public class VideoMetadata : IVideoMetadata
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Name to display
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Description to display below the film
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Length of a video to display on FilmsList
        /// </summary>
        public string Length { get; set; }
        /// <summary>
        /// Name of the Thumbnail file (including format)
        /// </summary>
        public string ThumbnailName { get; set; }
        /// <summary>
        /// URL of the blob storing the video
        /// </summary>
        public string BlobUrl { get; set; }
        /// <summary>
        /// Type of video, if null or default = it'll be treated as only for subscribers (paid)
        /// </summary>
        public string VideoType { get; set; }
        /// <summary>
        /// When this video has been uploaded
        /// </summary>
        public DateTimeOffset CreatedOn { get; set; }
    }
}
