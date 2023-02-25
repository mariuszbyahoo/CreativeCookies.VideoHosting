using CreativeCookies.VideoHosting.EfCore.Models;
using Microsoft.EntityFrameworkCore;

namespace CreativeCookies.VideoHosting.EfCore
{
    public class VideoHostingDbContext : DbContext
    {
        DbSet<Video> Videos { get; set; }
        DbSet<VideoSegment> VideoSegments { get; set; }

        public VideoHostingDbContext(DbContextOptions<VideoHostingDbContext> options) :
            base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Video>().ToTable(nameof(Video));
            modelBuilder.Entity<VideoSegment>().ToTable(nameof(VideoSegment));
        }
    }
}