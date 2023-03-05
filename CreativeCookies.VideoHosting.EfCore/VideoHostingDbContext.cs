﻿using CreativeCookies.VideoHosting.EfCore.Models;
using Microsoft.EntityFrameworkCore;

namespace CreativeCookies.VideoHosting.EfCore
{
    public class VideoHostingDbContext : DbContext
    {
        public DbSet<Video> Videos { get; set; }
        public DbSet<VideoSegment> VideoSegments { get; set; }

        public VideoHostingDbContext(DbContextOptions<VideoHostingDbContext> options) :
            base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Video>().HasMany(v => v.VideoSegments).WithOne(s => s.Video).HasForeignKey(s => s.VideoId);
        }
    }
}