using ECommerce.OrderManagement.Infrastructure.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.IntegrationTests
{
    public class CorrelationIdIntegrationTests
     : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public CorrelationIdIntegrationTests(
            CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetHealth_WhenCorrelationIdProvided_ReturnsSameId()
        {
            // Arrange
            const string correlationId = "integration-test-123";

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "/health");

            request.Headers.Add("X-Correlation-ID", correlationId);

            // Act
            using var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.True(
                response.Headers.TryGetValues(
                    "X-Correlation-ID",
                    out var values));

            Assert.Equal(correlationId, Assert.Single(values!));
        }

        [Fact]
        public async Task GetHealth_WhenCorrelationIdMissing_ReturnsGeneratedId()
        {
            // Arrange
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "/health");

            // Act
            using var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.True(
                response.Headers.TryGetValues(
                    "X-Correlation-ID",
                    out var values));

            var correlationId = Assert.Single(values!);

            Assert.True(
                Guid.TryParseExact(correlationId, "N", out _));
        }
    }
}
