using ECommerce.OrderManagement.Application.Authentication.DTOs;
using ECommerce.OrderManagement.Application.Requests;
using ECommerce.OrderManagement.Domain.Entities;
using ECommerce.OrderManagement.Infrastructure.Infrastructure;
using ECommerce.OrderManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.IntegrationTests.Orders
{
    public sealed class OrderCreationTests
    : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        private readonly HttpClient _client;
        public OrderCreationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task CreateOrder_ValidRequest_ReturnsCreatedAndDecreasesStock()
        {
            // Arrange
            using var client = _factory.CreateClient();

            var (userId, token) = await RegisterAndLoginAsync();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            await _factory.VerifyTestDatabasesAsync();

            var product = new Product(
                "Integration Keyboard",
                $"IT-KB-{Guid.NewGuid():N}",
                100m,
                10);

            Guid productId = product.Id;

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider
                    .GetRequiredService<ECommerceDbContext>();

                await dbContext.Products.AddAsync(product);
                await dbContext.SaveChangesAsync();
            }

            var request = new CreateOrderRequest(
                new[]
                {
                new CreateOrderItemRequest(productId, 2)
                });

            client.DefaultRequestHeaders.Add(
                "Idempotency-Key",
                $"order-test-{Guid.NewGuid():N}");

            // Act
            var response = await client.PostAsJsonAsync(
                "/api/Orders",
                request);

            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            using var json = JsonDocument.Parse(responseBody);
            var orderId = json.RootElement
                .GetProperty("id")
                .GetGuid();

            Assert.NotEqual(Guid.Empty, orderId);

            using var verifyScope = _factory.Services.CreateScope();

            var verifyDbContext = verifyScope.ServiceProvider
                .GetRequiredService<ECommerceDbContext>();

            var savedProduct = await verifyDbContext.Products
                .AsNoTracking()
                .SingleAsync(p => p.Id == productId);

            Assert.Equal(8, savedProduct.Stock);

            var savedOrder = await verifyDbContext.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .SingleAsync(o => o.Id == orderId);

            Assert.Equal(200m, savedOrder.TotalAmount);
            Assert.Single(savedOrder.Items);
        }

        [Fact]
        public async Task CreateOrder_SameIdempotencyKey_ReturnsSameOrderWithoutDecreasingStockAgain()
        {
            // Arrange
            using var client = _factory.CreateClient();

            var (userId, token) = await RegisterAndLoginAsync();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var product = new Product(
                "Idempotency Keyboard",
                $"IDEM-KB-{Guid.NewGuid():N}",
                100m,
                10);

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider
                    .GetRequiredService<ECommerceDbContext>();

                await dbContext.Products.AddAsync(product);
                await dbContext.SaveChangesAsync();
            }

            var request = new CreateOrderRequest(
                new[]
                {
            new CreateOrderItemRequest(product.Id, 2)
                });

            var idempotencyKey = $"idem-{Guid.NewGuid():N}";

            // Act
            using var firstRequest = new HttpRequestMessage(
                HttpMethod.Post,
                "/api/Orders")
            {
                Content = JsonContent.Create(request)
            };

            firstRequest.Headers.Add("Idempotency-Key", idempotencyKey);

            var firstResponse = await client.SendAsync(firstRequest);
            var firstBody = await firstResponse.Content.ReadAsStringAsync();

            // Act
            using var secondRequest = new HttpRequestMessage(
                HttpMethod.Post,
                "/api/Orders")
            {
                Content = JsonContent.Create(request)
            };

            secondRequest.Headers.Add("Idempotency-Key", idempotencyKey);

            var secondResponse = await client.SendAsync(secondRequest);
            var secondBody = await secondResponse.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);
            Assert.Equal(firstBody, secondBody);

            using var json = JsonDocument.Parse(firstBody);
            var orderId = json.RootElement
                .GetProperty("id")
                .GetGuid();

            using var verifyScope = _factory.Services.CreateScope();
            var verifyDbContext = verifyScope.ServiceProvider
                .GetRequiredService<ECommerceDbContext>();

            var savedProduct = await verifyDbContext.Products
                .AsNoTracking()
                .SingleAsync(p => p.Id == product.Id);

            Assert.Equal(8, savedProduct.Stock);

            var orderCount = await verifyDbContext.Orders
                .AsNoTracking()
                .CountAsync(o => o.Id == orderId);

            Assert.Equal(1, orderCount);
        }

        [Fact]
        public async Task CreateOrder_SameIdempotencyKeyWithDifferentRequest_ReturnsConflict()
        {
            // Arrange
            using var client = _factory.CreateClient();

            var (userId, token) = await RegisterAndLoginAsync();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var product = new Product(
                "Idempotency Conflict Keyboard",
                $"IDEM-CONFLICT-{Guid.NewGuid():N}",
                100m,
                10);

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider
                    .GetRequiredService<ECommerceDbContext>();

                await dbContext.Products.AddAsync(product);
                await dbContext.SaveChangesAsync();
            }

            var idempotencyKey = $"idem-conflict-{Guid.NewGuid():N}";

            var firstRequest = new CreateOrderRequest(
                new[]
                {
            new CreateOrderItemRequest(product.Id, 2)
                });

            var differentRequest = new CreateOrderRequest(
                new[]
                {
            new CreateOrderItemRequest(product.Id, 3)
                });

            // Act
            using var request1 = new HttpRequestMessage(
                HttpMethod.Post,
                "/api/Orders")
            {
                Content = JsonContent.Create(firstRequest)
            };

            request1.Headers.Add("Idempotency-Key", idempotencyKey);

            var response1 = await client.SendAsync(request1);

            // Act
            using var request2 = new HttpRequestMessage(
                HttpMethod.Post,
                "/api/Orders")
            {
                Content = JsonContent.Create(differentRequest)
            };

            request2.Headers.Add("Idempotency-Key", idempotencyKey);

            var response2 = await client.SendAsync(request2);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, response2.StatusCode);

            using var verifyScope = _factory.Services.CreateScope();
            var verifyDbContext = verifyScope.ServiceProvider
                .GetRequiredService<ECommerceDbContext>();

            var savedProduct = await verifyDbContext.Products
                .AsNoTracking()
                .SingleAsync(p => p.Id == product.Id);

            Assert.Equal(8, savedProduct.Stock);

            var orderCount = await verifyDbContext.Orders
                .AsNoTracking()
                .CountAsync(o => o.Items.Any(i => i.ProductId == product.Id));

            Assert.Equal(1, orderCount);
        }

        [Fact]
        public async Task CreateOrder_ConcurrentRequestsWithSameKey_CreatesOnlyOneOrder()
        {
            // Arrange
            using var client = _factory.CreateClient();

            var (userId, token) = await RegisterAndLoginAsync();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var product = new Product(
                "Concurrent Keyboard",
                $"CONCURRENT-{Guid.NewGuid():N}",
                100m,
                10);

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider
                    .GetRequiredService<ECommerceDbContext>();

                await dbContext.Products.AddAsync(product);
                await dbContext.SaveChangesAsync();
            }

            var request = new CreateOrderRequest(
                new[]
                {
            new CreateOrderItemRequest(product.Id, 2)
                });

            var idempotencyKey = $"concurrent-{Guid.NewGuid():N}";

            async Task<HttpResponseMessage> SendRequestAsync()
            {
                using var message = new HttpRequestMessage(
                    HttpMethod.Post,
                    "/api/Orders")
                {
                    Content = JsonContent.Create(request)
                };

                message.Headers.Add("Idempotency-Key", idempotencyKey);

                return await client.SendAsync(message);
            }

            // Act
            var responses = await Task.WhenAll(
                SendRequestAsync(),
                SendRequestAsync());

            // Assert
            Assert.All(
                responses,
                response => Assert.Equal(
                    HttpStatusCode.Created,
                    response.StatusCode));

            var bodies = await Task.WhenAll(
                responses.Select(response =>
                    response.Content.ReadAsStringAsync()));

            Assert.Equal(bodies[0], bodies[1]);

            using var verifyScope = _factory.Services.CreateScope();

            var verifyDbContext = verifyScope.ServiceProvider
                .GetRequiredService<ECommerceDbContext>();

            var savedProduct = await verifyDbContext.Products
                .AsNoTracking()
                .SingleAsync(p => p.Id == product.Id);

            Assert.Equal(8, savedProduct.Stock);

            var orderCount = await verifyDbContext.Orders
                .AsNoTracking()
                .CountAsync(o => o.Items.Any(i => i.ProductId == product.Id));

            Assert.Equal(1, orderCount);
        }

        [Fact]
        public async Task CreateOrder_ConcurrentOrdersForLimitedStock_DoesNotOversell()
        {
            using var client = _factory.CreateClient();

            var (userId, token) = await RegisterAndLoginAsync();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var product = new Product(
                "Limited Stock Keyboard",
                $"LIMITED-{Guid.NewGuid():N}",
                100m,
                5);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider
                    .GetRequiredService<ECommerceDbContext>();

                await db.Products.AddAsync(product);
                await db.SaveChangesAsync();
            }

            async Task<HttpResponseMessage> CreateOrderAsync()
            {
                var request = new CreateOrderRequest(
                    new[]
                    {
                        new CreateOrderItemRequest(product.Id, 4)
                    });

                using var message = new HttpRequestMessage(
                    HttpMethod.Post,
                    "/api/Orders")
                {
                    Content = JsonContent.Create(request)
                };

                message.Headers.Add(
                    "Idempotency-Key",
                    $"stock-race-{Guid.NewGuid():N}");

                return await client.SendAsync(message);
            }

            var responses = await Task.WhenAll(
                CreateOrderAsync(),
                CreateOrderAsync());

            using var verifyScope = _factory.Services.CreateScope();

            var verifyDb = verifyScope.ServiceProvider
                .GetRequiredService<ECommerceDbContext>();

            var savedProduct = await verifyDb.Products
                .AsNoTracking()
                .SingleAsync(p => p.Id == product.Id);

            var orders = await verifyDb.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .Where(o => o.Items.Any(i => i.ProductId == product.Id))
                .ToListAsync();

            Assert.All(
                responses,
                response => Assert.Contains(
                    (int)response.StatusCode,
                    new[] { 201, 409 }));

            Assert.Single(
                responses.Where(r => r.StatusCode == HttpStatusCode.Created));

            Assert.Equal(1, orders.Count);
            Assert.Equal(
                4,
                orders.Sum(o =>
                    o.Items
                        .Where(i => i.ProductId == product.Id)
                        .Sum(i => i.Quantity)));
            Assert.Equal(1, savedProduct.Stock);
}
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
        public async Task CreateOrder_WhenRequestLimitExceeded_Returns429()
        {
            // Arrange
            using var client = _factory.CreateClient();

            var (_, token) = await RegisterAndLoginAsync();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var product = new Product(
                "Rate Limit Test Product",
                $"RATE-{Guid.NewGuid():N}",
                100m,
                100);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider
                    .GetRequiredService<ECommerceDbContext>();

                await db.Products.AddAsync(product);
                await db.SaveChangesAsync();
            }

            // Act
            var responses = new List<HttpResponseMessage>();

            for (var i = 0; i < 6; i++)
            {
                var request = new CreateOrderRequest(
                    new[]
                    {
                new CreateOrderItemRequest(product.Id, 1)
                    });

                using var message = new HttpRequestMessage(
                    HttpMethod.Post,
                    "/api/Orders")
                {
                    Content = JsonContent.Create(request)
                };

                message.Headers.Add(
                    "Idempotency-Key",
                    $"rate-limit-{Guid.NewGuid():N}");

                responses.Add(await client.SendAsync(message));
            }
  
            // Assert
            try
            {
                Assert.Equal(
                    HttpStatusCode.TooManyRequests,
                    responses[5].StatusCode);

                Assert.True(
                    responses[5].Headers.Contains("Retry-After"),
                    "Expected Retry-After header in the 429 response.");

                var retryAfterValue =
                    responses[5].Headers.GetValues("Retry-After").Single();

                Assert.True(
                    int.TryParse(retryAfterValue, out var retryAfterSeconds),
                    $"Retry-After should be an integer, but received: {retryAfterValue}");

                Assert.True(
                    retryAfterSeconds > 0 && retryAfterSeconds <= 60,
                    $"Retry-After should be between 1 and 60 seconds, but received: {retryAfterSeconds}");

                Assert.All(
                    responses.Take(5),
                    response => Assert.NotEqual(
                        HttpStatusCode.TooManyRequests,
                        response.StatusCode));
            }
            finally
            {
                foreach (var response in responses)
                {
                    response.Dispose();
                }
            }
        }

        [Fact]
        public async Task Health_ReturnsHealthy()
        {
            using var client = _factory.CreateClient();

            var response = await client.GetAsync("/health");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal("Healthy", content);
        }

        [Fact]
        public async Task HealthReady_ReturnsHealthy_WhenDatabaseIsAvailable()
        {
            using var client = _factory.CreateClient();

            var response = await client.GetAsync("/health/ready");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal("Healthy", content);
        }

        [Fact]
        public async Task Health_WhenCorrelationIdProvided_ReturnsSameId()
        {
            using var client = _factory.CreateClient();

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "/health");

            request.Headers.Add(
                "X-Correlation-ID",
                "test-correlation-123");

            using var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            Assert.True(
                response.Headers.TryGetValues(
                    "X-Correlation-ID",
                    out var values));

            Assert.Equal(
                "test-correlation-123",
                values!.Single());
        }

        [Fact]
        public async Task Health_WhenCorrelationIdMissing_GeneratesId()
        {
            using var client = _factory.CreateClient();

            using var response = await client.GetAsync("/health");

            response.EnsureSuccessStatusCode();

            Assert.True(
                response.Headers.TryGetValues(
                    "X-Correlation-ID",
                    out var values));

            var correlationId = values!.Single();

            Assert.False(string.IsNullOrWhiteSpace(correlationId));
            Assert.True(Guid.TryParseExact(correlationId, "N", out _));
        }

    }
}

