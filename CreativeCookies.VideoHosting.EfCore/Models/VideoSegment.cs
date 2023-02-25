using CreativeCookies.VideoHosting.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.EfCore.Models
{
    public class VideoSegment : BaseEntity, IVideoSegment
    {
        public Guid VideoId { get; set; }
        public int SequenceNumber { get; set; }
        public byte[] Data { get; set; }
    }
}
