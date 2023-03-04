﻿using CreativeCookies.VideoHosting.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.EfCore.Models
{
    public class VideoSegment : IVideoSegment
    {
        public Guid Id { get; }
        public DateTime DateCreated { get; }
        public DateTime DateUpdated { get; set; }
        public Guid VideoId { get; set; }
        public int SequenceNumber { get; set; }
        public byte[] Data { get; set; }

        public VideoSegment()
        {
            Id = Guid.NewGuid();
            DateCreated = DateTime.Now;
        }
    }
}
