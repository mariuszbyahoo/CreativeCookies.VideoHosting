﻿// <auto-generated />
using System;
using CreativeCookies.VideoHosting.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CreativeCookies.VideoHosting.EfCore.Migrations
{
    [DbContext(typeof(VideoHostingDbContext))]
    [Migration("20230305140811_third")]
    partial class third
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("CreativeCookies.VideoHosting.EfCore.Models.Video", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("DateUpdated")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Duration")
                        .HasColumnType("int");

                    b.Property<string>("Location")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Videos");
                });

            modelBuilder.Entity("CreativeCookies.VideoHosting.EfCore.Models.VideoSegment", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<byte[]>("Data")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<DateTime>("DateUpdated")
                        .HasColumnType("datetime2");

                    b.Property<int>("SequenceNumber")
                        .HasColumnType("int");

                    b.Property<Guid>("VideoId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("VideoId");

                    b.ToTable("VideoSegments");
                });

            modelBuilder.Entity("CreativeCookies.VideoHosting.EfCore.Models.VideoSegment", b =>
                {
                    b.HasOne("CreativeCookies.VideoHosting.EfCore.Models.Video", "Video")
                        .WithMany("VideoSegments")
                        .HasForeignKey("VideoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Video");
                });

            modelBuilder.Entity("CreativeCookies.VideoHosting.EfCore.Models.Video", b =>
                {
                    b.Navigation("VideoSegments");
                });
#pragma warning restore 612, 618
        }
    }
}
