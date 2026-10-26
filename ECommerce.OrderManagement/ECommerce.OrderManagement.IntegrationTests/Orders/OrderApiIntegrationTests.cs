using ECommerce.OrderManagement.Application.Authentication.DTOs;
using ECommerce.OrderManagement.Application.Exceptions;
using ECommerce.OrderManagement.Domain.Entities;
using ECommerce.OrderManagement.Domain.Enums;
using ECommerce.OrderManagement.Infrastructure.Infrastructure;
using ECommerce.OrderManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.IntegrationTests.Orders
{
    [Collection("Integration Tests")]
    public sealed class OrderApiIntegrationTests
     : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public OrderApiIntegrationTests(
            CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetOrderById_WhenOrderDoesNotExist_Returns404ProblemDetails()
        {
            var (_, token) = await RegisterAndLoginAsync();

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", token);

            var orderId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync(
                $"/api/Orders/{orderId}");

            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Status Code: {(int)response.StatusCode}");
            Console.WriteLine($"Content-Type: {response.Content.Headers.ContentType}");
            Console.WriteLine($"Response Body: [{responseBody}]");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var problem = System.Text.Json.JsonSerializer
                .Deserialize<ProblemDetails>(responseBody);

            Assert.NotNull(problem);
            Assert.Equal(404, problem.Status);
            Assert.Equal("Not Found", problem.Title);
            Assert.Equal($"/api/Orders/{orderId}", problem.Instance);
        }

        [Fact]
        public async Task GetOrderById_WhenOrderBelongsToCurrentUser_Returns200()
        {
            // Arrange
            var (userId, token) = await RegisterAndLoginAsync();

            var order = Order.Create(userId);

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider
                    .GetRequiredService<ECommerceDbContext>();

                dbContext.Orders.Add(order);
                await dbContext.SaveChangesAsync();
            }

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", token);

            // Act
            var response = await _client.GetAsync(
                $"/api/Orders/{order.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }


        [Fact]
        public async Task GetOrderById_WhenOrderBelongsToAnotherUser_Returns404()
        {
            var (ownerUserId, _) = await RegisterAndLoginAsync();
            var order = Order.Create(ownerUserId);

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider
                    .GetRequiredService<ECommerceDbContext>();

                dbContext.Orders.Add(order);
                await dbContext.SaveChangesAsync();
            }

            var (_, anotherUserToken) = await RegisterAndLoginAsync();

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", anotherUserToken);

            // Act
            var response = await _client.GetAsync(
                $"/api/Orders/{order.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CancelOrder_WhenOrderBelongsToAnotherUser_Returns404()
        {
            var (ownerUserId, _) = await RegisterAndLoginAsync();

            var product = new Product(
                "Integration Test Keyboard",
                $"IT-{Guid.NewGuid():N}",
                100m,
                10);

            var order = Order.Create(ownerUserId);

            order.AddItem(new OrderItem(
                product.Id,
                product.Name,
                product.Sku,
                product.Price,
                2));

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider
                    .GetRequiredService<ECommerceDbContext>();

                dbContext.Products.Add(product);
                dbContext.Orders.Add(order);

                await dbContext.SaveChangesAsync();
            }

            var originalStock = product.Stock;

            var (_, anotherUserToken) = await RegisterAndLoginAsync();

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", anotherUserToken);

            // Act
            var response = await _client.PostAsync(
                $"/api/Orders/{order.Id}/cancel",
                content: null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            using var verifyScope = _factory.Services.CreateScope();

            var verifyDbContext = verifyScope.ServiceProvider
                .GetRequiredService<ECommerceDbContext>();

            var savedOrder = await verifyDbContext.Orders
                .AsNoTracking()
                .SingleAsync(x => x.Id == order.Id);

            var savedProduct = await verifyDbContext.Products
                .AsNoTracking()
                .SingleAsync(x => x.Id == product.Id);

            Assert.Equal(OrderStatus.Pending, savedOrder.Status);
            Assert.Equal(originalStock, savedProduct.Stock);
        }

        //private async Task<(Guid UserId, string Token)> RegisterAndLoginAsync()
        //{
        //    var uniqueId = Guid.NewGuid().ToString("N");
        //    var email = $"integration-{uniqueId}@example.com";
        //    var password = "TestPassword123!";

        //    var registerRequest = new RegisterRequest(
        //        $"integration-{uniqueId}",
        //        email,
        //        password);

        //    var registerResponse = await _client.PostAsJsonAsync(
        //        "/api/auth/register",
        //        registerRequest);

        //    registerResponse.EnsureSuccessStatusCode();

        //    var loginRequest = new LoginRequest(email, password);

        //    var loginResponse = await _client.PostAsJsonAsync(
        //        "/api/auth/login",
        //        loginRequest);

        //    loginResponse.EnsureSuccessStatusCode();

        //    var login = await loginResponse.Content
        //        .ReadFromJsonAsync<LoginResponse>();

        //    Assert.NotNull(login);

        //    return (login.UserId, login.AccessToken);
        //}

        private async Task<(Guid UserId, string Token)> RegisterAndLoginAsync()
        {
            var uniqueId = Guid.NewGuid().ToString("N");
            var email = $"integration-{uniqueId}@example.com";
            var password = "TestPassword123!";

            var registerRequest = new RegisterRequest(
                $"integration-{uniqueId}",
                email,
                password);

            var registerResponse = await _client.PostAsJsonAsync(
                "/api/auth/register",
                registerRequest);

            var registerBody = await registerResponse.Content
                .ReadAsStringAsync();

            if (!registerResponse.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Register failed: {(int)registerResponse.StatusCode} " +
                    $"{registerResponse.StatusCode}. Body: {registerBody}");
            }

            var loginRequest = new LoginRequest(email, password);

            var loginResponse = await _client.PostAsJsonAsync(
                "/api/auth/login",
                loginRequest);

            var loginBody = await loginResponse.Content
                .ReadAsStringAsync();

            if (!loginResponse.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Login failed: {(int)loginResponse.StatusCode} " +
                    $"{loginResponse.StatusCode}. Body: {loginBody}");
            }

            var login = await loginResponse.Content
                .ReadFromJsonAsync<LoginResponse>();

            Assert.NotNull(login);

            return (login.UserId, login.AccessToken);
        }

        [Fact]
        public async Task GetOrders_WhenUserIsNotAuthenticated_Returns401()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.GetAsync("/api/Orders");

            // Assert
            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }


        [Fact]
        public async Task CancelOrder_WhenOrderBelongsToCurrentUser_Returns204AndRestoresStock()
        {
            // Arrange
            var (userId, token) = await RegisterAndLoginAsync();

            var product = new Product(
                "Integration Test Keyboard",
                $"IT-{Guid.NewGuid():N}",
                100m,
                10);

            var order = Order.Create(userId);

            const int quantity = 2;

            order.AddItem(new OrderItem(
                product.Id,
                product.Name,
                product.Sku,
                product.Price,
                quantity));

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider
                    .GetRequiredService<ECommerceDbContext>();

                dbContext.Products.Add(product);
                dbContext.Orders.Add(order);

                await dbContext.SaveChangesAsync();
            }

            var originalStock = product.Stock;

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer",
                    token);

            // Act
            var response = await _client.PostAsync(
                $"/api/Orders/{order.Id}/cancel",
                content: null);

            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Status Code: {(int)response.StatusCode}");
            Console.WriteLine($"Response Body: {responseBody}");

            Assert.True(
                response.StatusCode == HttpStatusCode.NoContent,
                $"Expected 204 NoContent, actual {(int)response.StatusCode}. " +
                $"Response body: {responseBody}");

            // Assert: database state
            using var verifyScope = _factory.Services.CreateScope();

            var verifyDbContext = verifyScope.ServiceProvider
                .GetRequiredService<ECommerceDbContext>();

            var savedOrder = await verifyDbContext.Orders
                .AsNoTracking()
                .SingleAsync(x => x.Id == order.Id);

            var savedProduct = await verifyDbContext.Products
                .AsNoTracking()
                .SingleAsync(x => x.Id == product.Id);

            Assert.Equal(OrderStatus.Cancelled, savedOrder.Status);
            Assert.Equal(originalStock + quantity, savedProduct.Stock);
        }

    }
}
