using CreativeCookies.VideoHosting.Contracts.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;

namespace CreativeCookies.VideoHosting.API.IntegrationTests
{
    public class Tests
    {
        private readonly WebApplicationFactory<Program> _factory = new WebApplicationFactory<Program>();

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task ControllerRoute_AfterReceivingGET_Returns200WithAnEmptyArray()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act 
            var response = await client.GetAsync("api/Videos");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = JsonConvert.DeserializeObject<IEnumerable<IVideo>>(await response.Content.ReadAsStringAsync());
            Assert.That(content, Is.EqualTo(new IVideo[0] ));
        }
    }
}