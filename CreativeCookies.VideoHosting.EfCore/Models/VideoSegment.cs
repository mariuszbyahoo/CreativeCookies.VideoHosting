using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.EfCore.Models
{
    public class VideoSegment : BaseEntity
    {
        public Guid VideoId { get; set; }
        public int SequenceNumber { get; set; }
        public byte[] Data { get; set; }

        public VideoSegment(Guid videoId, int sequenceNumber, byte[] data) :
            base()
        {
            VideoId = videoId;
            SequenceNumber = sequenceNumber;
            Data = data;
        }
    }
}
