using ECommerce.OrderManagement.Infrastructure.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.IntegrationTests
{
    public sealed class OrderRoleAuthorizationTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public OrderRoleAuthorizationTests(
            CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CancelOrder_WithAdminRole_PassesAuthorization()
        {
            using var scope = _factory.Services.CreateScope();

            var configuration = scope.ServiceProvider
                .GetRequiredService<IConfiguration>();

            using var client = _factory.CreateClient();

            var token = TestJwtTokenFactory.CreateToken(
                configuration,
                "Admin");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var orderId = Guid.NewGuid();

            var response = await client.PostAsync(
                $"/api/Orders/{orderId}/cancel",
                content: null);

            Assert.NotEqual(
                HttpStatusCode.Unauthorized,
                response.StatusCode);

            Assert.NotEqual(
                HttpStatusCode.Forbidden,
                response.StatusCode);
        }
        [Fact]
        public async Task CancelOrder_WithoutToken_ReturnsUnauthorized()
        {
            using var client = _factory.CreateClient();

            var orderId = Guid.NewGuid();

            var response = await client.PostAsync(
                $"/api/Orders/{orderId}/cancel",
                content: null);

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }
    }
}
