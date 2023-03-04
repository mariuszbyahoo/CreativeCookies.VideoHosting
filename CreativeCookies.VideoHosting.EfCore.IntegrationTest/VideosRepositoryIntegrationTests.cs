using CreativeCookies.VideoHosting.EfCore.Models;
using CreativeCookies.VideoHosting.EfCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CreativeCookies.VideoHosting.EfCore.IntegrationTest
{
    public class VideosRepositoryIntegrationTests
    {
        VideosRepository Repository { get; set; }

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<VideoHostingDbContext>()
                .UseSqlServer("Server=MARIUSZDESKTOP;Database=CreativeCookies_VideoHosting;Trusted_Connection=True;TrustServerCertificate=True")
                .Options;
            Repository = new VideosRepository(new VideoHostingDbContext(options));
        }

        [Test]
        public async Task PostVideo_WhenValidVideoSupplied_ReturnsSameVideoAsSuccessResult()
        {
            var video = new Video("InsurgencyFnFal.mp4", "Sample video from InsurgencySandstorm", "C:\\SampleVideo");
            var result = (Video) await Repository.PostVideo(video);

            Assert.That(result, Is.SameAs(video));
        }
    }
}