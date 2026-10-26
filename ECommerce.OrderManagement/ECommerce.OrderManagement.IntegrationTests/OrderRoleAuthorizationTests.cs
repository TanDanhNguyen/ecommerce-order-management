using ECommerce.OrderManagement.Infrastructure.Infrastructure;
using Microsoft.Extensions.Configuration;
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
        public async Task CancelOrder_WithCustomerRole_ReturnsForbidden()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            using var client = _factory.CreateClient();

            var token = TestJwtTokenFactory.CreateToken(
                configuration,
                "Customer");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var orderId = Guid.NewGuid();

            var response = await client.PostAsync(
                $"/api/Orders/{orderId}/cancel",
                content: null);

            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode);
        }
    }
}
