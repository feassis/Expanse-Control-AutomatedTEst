using System.Net;
using FluentAssertions;
using Xunit;

namespace IntegrationTests
{
    public class SmokeTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public SmokeTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task API_Deve_Subir()
        {
            var response = await _client.GetAsync("/");

            response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        }
    }
}
