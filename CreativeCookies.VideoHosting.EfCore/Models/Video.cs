using CreativeCookies.VideoHosting.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.EfCore.Models
{
    public class Video : BaseEntity, IVideo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public int Duration { get; set; }
        public ICollection<VideoSegment> VideoSegments { get; set; }
    }
}
