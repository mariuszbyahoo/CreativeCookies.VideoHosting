using Microsoft.AspNetCore.Mvc.Testing;

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
        public async Task ControllerRoute_AfterReceivingGET_Returns200WithDesiredMessage()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act 
            var response = await client.GetAsync("api/Videos");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.That(content, Is.EqualTo("Yep everything running as it should..."));
        }
    }
}