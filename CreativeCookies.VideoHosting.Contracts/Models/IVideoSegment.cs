using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.Contracts.Models
{
    public interface IVideoSegment
    {
        Guid Id { get; }
        Guid VideoId { get; set; }
        int SequenceNumber { get; set; }
        byte[] Data { get; set; }
    }
}
