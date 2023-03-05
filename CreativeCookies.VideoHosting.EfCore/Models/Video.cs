using CreativeCookies.VideoHosting.Contracts.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.EfCore.Models
{
    [PrimaryKey(nameof(Id))]
    public class Video : IVideo
    {
        public Guid Id { get; }
        public DateTime DateCreated { get; }
        public DateTime DateUpdated { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public int Duration { get; set; }
        public ICollection<VideoSegment> VideoSegments { get; set; }

        public Video()
        {
            Id = Guid.NewGuid();
            DateCreated = DateTime.Now;
        }

        public Video(string name, string description, string location)
        {
            Id = Guid.NewGuid();
            DateCreated = DateTime.Now;
            Name = name;
            Description = description;
            Location = location;
            VideoSegments = new List<VideoSegment>();
        }
    }
}
