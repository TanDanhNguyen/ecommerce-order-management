using ECommerce.OrderManagement.Infrastructure.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.IntegrationTests
{
    public sealed class ProductValidationTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ProductValidationTests(
            CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateProduct_WithInvalidPrice_ReturnsBadRequestWithProblemDetails()
        {
            // Arrange
            var request = new
            {
                Name = "Test Product",
                Sku = $"TEST-{Guid.NewGuid():N}",
                Price = 0,
                Stock = 10
            };

            // Act
            var response = await _client.PostAsJsonAsync(
                "/api/Products",
                request);

            var body = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(
                HttpStatusCode.BadRequest,
                response.StatusCode);

            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            Assert.Equal(
                400,
                root.GetProperty("status").GetInt32());

            Assert.Equal(
                "Validation failed.",
                root.GetProperty("title").GetString());

            Assert.True(root.TryGetProperty("errors", out var errors));
            Assert.True(errors.TryGetProperty("Price", out _));

            Assert.True(root.TryGetProperty("traceId", out var traceId));
            Assert.False(string.IsNullOrWhiteSpace(traceId.GetString()));
        }

        [Fact]
        public async Task AdjustStock_WithInvalidOperation_ReturnsBadRequest()
        {
            var productId = Guid.NewGuid();

            var request = new
            {
                Quantity = 1,
                Operation = "decrese"
            };

            var response = await _client.PatchAsJsonAsync(
                $"/api/Products/{productId}/stock",
                request);

            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(
                HttpStatusCode.BadRequest,
                response.StatusCode);

            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            Assert.True(
                root.GetProperty("errors").TryGetProperty("Operation", out _));
        }

        [Fact]
        public async Task UpdateProduct_WithInvalidPrice_ReturnsBadRequestWithProblemDetails()
        {
            // Arrange
            var productId = Guid.NewGuid();

            var request = new
            {
                Name = "Updated Product",
                Sku = $"UPDATE-{Guid.NewGuid():N}",
                Price = 0,
                Stock = 10
            };

            // Act
            var response = await _client.PutAsJsonAsync(
                $"/api/Products/{productId}",
                request);

            var body = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(
                HttpStatusCode.BadRequest,
                response.StatusCode);

            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            Assert.Equal(400, root.GetProperty("status").GetInt32());

            Assert.True(
                root.GetProperty("errors").TryGetProperty("Price", out _));

            Assert.True(root.TryGetProperty("traceId", out var traceId));
            Assert.False(string.IsNullOrWhiteSpace(traceId.GetString()));
        }

    }
}
