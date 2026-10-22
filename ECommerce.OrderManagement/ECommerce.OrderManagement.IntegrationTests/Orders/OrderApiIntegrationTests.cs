using ECommerce.OrderManagement.Infrastructure.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.IntegrationTests.Orders
{
    public sealed class OrderApiIntegrationTests
     : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public OrderApiIntegrationTests(
            CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetOrderById_WhenOrderDoesNotExist_Returns404ProblemDetails()
        {
            // Arrange
            var orderId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync(
                $"/api/Orders/{orderId}");

            var problem = await response.Content
                .ReadFromJsonAsync<ProblemDetails>();

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(problem);
            Assert.Equal(404, problem.Status);
            Assert.Equal("Not Found", problem.Title);
            Assert.Equal($"/api/Orders/{orderId}", problem.Instance);
        }
    }
}
