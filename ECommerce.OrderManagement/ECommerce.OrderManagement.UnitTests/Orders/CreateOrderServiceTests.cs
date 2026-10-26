using ECommerce.OrderManagement.Application.Authentication.Interfaces;
using ECommerce.OrderManagement.Application.Exceptions;
using ECommerce.OrderManagement.Application.Idempotency;
using ECommerce.OrderManagement.Application.Interfaces;
using ECommerce.OrderManagement.Application.Orders;
using ECommerce.OrderManagement.Application.Orders.DTOs;
using ECommerce.OrderManagement.Application.Requests;
using ECommerce.OrderManagement.Domain.Entities;
using ECommerce.OrderManagement.Domain.Enums;
using Moq;
using System.Text.Json;

namespace ECommerce.OrderManagement.UnitTests.Orders
{
    public class CreateOrderServiceTests
    {
        [Fact]
        public async Task CreateAsync_ValidRequest_CreatesOrderAndDecreasesStock()
        {
            // Arrange
            var product = new Product(
                "Keyboard",
                "KB-001",
                100m,
                10);

            var productRepository = new Mock<IProductRepository>();

            productRepository
                .Setup(x => x.GetByIdsAsync(
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (IReadOnlyCollection<Guid> ids, CancellationToken _) =>
                        (IReadOnlyList<Product>)new[] { product });

            var orderRepository = new Mock<IOrderRepository>();

            orderRepository
                .Setup(x => x.AddAsync(
                    It.IsAny<Order>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var transaction = new Mock<IOrderTransaction>();

            transaction
                .Setup(x => x.ExecuteAsync(
                    It.IsAny<Func<CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()))
                .Returns((
                    Func<CancellationToken, Task> operation,
                    CancellationToken ct) => operation(ct));

            var userId = Guid.NewGuid();

            var currentUserService = new Mock<ICurrentUserService>();
            currentUserService
                .Setup(x => x.UserId)
                .Returns(userId);

            var stockMovementRepository = new Mock<IStockMovementRepository>();
            var idempotencyRepository = new Mock<IIdempotencyRepository>();
            var service = new CreateOrderService(
                productRepository.Object,
                orderRepository.Object,
                transaction.Object,
                currentUserService.Object,
                stockMovementRepository.Object,
                idempotencyRepository.Object);

            var request = new CreateOrderRequest(
                new[]
                {
                new CreateOrderItemRequest(product.Id, 2)
                });

            // Act
            var result = await service.CreateAsync(
                request,
                "test-order-success-001",
                CancellationToken.None);

            var order = JsonSerializer.Deserialize<CreateOrderResponse>(
                result.ResponseBody,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));

            // Assert
            Assert.NotNull(order);
            Assert.Equal(201, result.StatusCode);
            Assert.False(result.IsReplay);

            Assert.Equal(nameof(OrderStatus.Pending), order.Status);
            Assert.Single(order.Items);
            Assert.Equal(200m, order.TotalAmount);
            Assert.Equal(8, product.Stock);

            orderRepository.Verify(
                x => x.AddAsync(
                    It.Is<Order>(o => o.TotalAmount == 200m),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }


        [Fact]
        public async Task CreateAsync_InsufficientStock_ThrowsInsufficientStockException()
        {
            // Arrange
            var product = new Product("Keyboard", "KB-002", 100m, 2);
            var productRepository = CreateProductRepository(product);
            var orderRepository = new Mock<IOrderRepository>();
            var transaction = new Mock<IOrderTransaction>();

            var userId = Guid.NewGuid();

            var currentUserService = new Mock<ICurrentUserService>();
            currentUserService
                .Setup(x => x.UserId)
                .Returns(userId);

            var stockMovementRepository = new Mock<IStockMovementRepository>();
            var idempotencyRepository = new Mock<IIdempotencyRepository>(); 
            var service = new CreateOrderService(
                productRepository.Object,
                orderRepository.Object,
                transaction.Object,
                currentUserService.Object,
                stockMovementRepository.Object,
                idempotencyRepository.Object);

            var request = new CreateOrderRequest(
                new[]
                {
            new CreateOrderItemRequest(product.Id, 3)
                });

            // Act & Assert
            await Assert.ThrowsAsync<InsufficientStockException>(
              () => service.CreateAsync(
                  request,
                  "test-idempotency-key",
                  CancellationToken.None));

            Assert.Equal(2, product.Stock);

            orderRepository.Verify(
                x => x.AddAsync(
                    It.IsAny<Order>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateAsync_ProductNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var productRepository = CreateProductRepository();
            var orderRepository = new Mock<IOrderRepository>();
            var transaction = new Mock<IOrderTransaction>();

            var userId = Guid.NewGuid();

            var currentUserService = new Mock<ICurrentUserService>();
            currentUserService
                .Setup(x => x.UserId)
                .Returns(userId);

            var stockMovementRepository = new Mock<IStockMovementRepository>();
            var idempotencyRepository = new Mock<IIdempotencyRepository>();
            var service = new CreateOrderService(
                productRepository.Object,
                orderRepository.Object,
                transaction.Object,
                currentUserService.Object,
                stockMovementRepository.Object,
                idempotencyRepository.Object);

            var request = new CreateOrderRequest(
                new[]
                {
            new CreateOrderItemRequest(Guid.NewGuid(), 1)
                });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(
                    request,
                    "test-product-not-found-001",
                    CancellationToken.None));

            orderRepository.Verify(
                x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
        [Fact]
        public async Task CreateAsync_EmptyItems_ThrowsArgumentException()
        {
            // Arrange
            var productRepository = CreateProductRepository();
            var orderRepository = new Mock<IOrderRepository>();
            var transaction = new Mock<IOrderTransaction>();

            var userId = Guid.NewGuid();

            var currentUserService = new Mock<ICurrentUserService>();
            currentUserService
                .Setup(x => x.UserId)
                .Returns(userId);

            var stockMovementRepository =
                new Mock<IStockMovementRepository>();
            var idempotencyRepository = new Mock<IIdempotencyRepository>();
            var service = new CreateOrderService(
                productRepository.Object,
                orderRepository.Object,
                transaction.Object,
                currentUserService.Object,
                stockMovementRepository.Object,
                idempotencyRepository.Object);

            var request = new CreateOrderRequest(
                Array.Empty<CreateOrderItemRequest>());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.CreateAsync(
                    request,
                    "test-empty-items-001",
                    CancellationToken.None));

            transaction.Verify(
                x => x.ExecuteAsync(
                    It.IsAny<Func<CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateAsync_ZeroQuantity_ThrowsArgumentException()
        {
            // Arrange
            var product = new Product("Keyboard", "KB-001", 100m, 10);

            var productRepository = CreateProductRepository(product);
            var orderRepository = new Mock<IOrderRepository>();
            var transaction = new Mock<IOrderTransaction>();

            var userId = Guid.NewGuid();

            var currentUserService = new Mock<ICurrentUserService>();
            currentUserService
                .Setup(x => x.UserId)
                .Returns(userId);

            var stockMovementRepository =
               new Mock<IStockMovementRepository>();
            var idempotencyRepository = new Mock<IIdempotencyRepository>();
            var service = new CreateOrderService(
                productRepository.Object,
                orderRepository.Object,
                transaction.Object,
                currentUserService.Object,
                stockMovementRepository.Object,
                idempotencyRepository.Object);

            var request = new CreateOrderRequest(
                new[]
                {
            new CreateOrderItemRequest(product.Id, 0)
                });

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.CreateAsync(
                    request,
                    "test-idempotency-key",
                    CancellationToken.None));

            Assert.Equal(10, product.Stock);
        }

        [Fact]
        public async Task CreateAsync_NullRequest_ThrowsArgumentNullException()
        {


            var userId = Guid.NewGuid();

            var currentUserService = new Mock<ICurrentUserService>();
            currentUserService
                .Setup(x => x.UserId)
                .Returns(userId);
            var stockMovementRepository = new Mock<IStockMovementRepository>();
            var idempotencyRepository = new Mock<IIdempotencyRepository>(); 
            // Arrange
            var service = new CreateOrderService(
                CreateProductRepository().Object,
                new Mock<IOrderRepository>().Object,
                new Mock<IOrderTransaction>().Object,
                currentUserService.Object,
                stockMovementRepository.Object,
                idempotencyRepository.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => service.CreateAsync(
                    null!,
                    "test-idempotency-key",
                    CancellationToken.None));
        }

        private static Mock<IProductRepository> CreateProductRepository(
                params Product[] products)
            {
            var repository = new Mock<IProductRepository>();

            repository
                .Setup(x => x.GetByIdsAsync(
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    (IReadOnlyCollection<Guid> ids, CancellationToken _) =>
                        (IReadOnlyList<Product>)products
                            .Where(p => ids.Contains(p.Id))
                            .ToArray());

            return repository;
        }
        [Fact]
        public async Task CreateAsync_SameKeyAndSameRequest_ReturnsSavedResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            const string idempotencyKey = "replay-test-001";
            const string savedResponse = """{"orderId":"saved-order"}""";

            var product = new Product(
                "Keyboard",
                "KB-001",
                100m,
                10);

            var request = new CreateOrderRequest(
                new[]
                {
            new CreateOrderItemRequest(product.Id, 2)
                });

            var requestHash = RequestHashHelper.Compute(request);
            var now = DateTime.UtcNow;

            var existingRecord = new IdempotencyRecord(
                userId,
                idempotencyKey,
                requestHash,
                201,
                savedResponse,
                now,
                now.AddHours(24));

            var productRepository = new Mock<IProductRepository>();
            var orderRepository = new Mock<IOrderRepository>();
            var transaction = new Mock<IOrderTransaction>();
            var stockMovementRepository = new Mock<IStockMovementRepository>();
            var idempotencyRepository = new Mock<IIdempotencyRepository>();

            var currentUserService = new Mock<ICurrentUserService>();
            currentUserService
                .Setup(x => x.UserId)
                .Returns(userId);

            idempotencyRepository
                .Setup(x => x.GetByUserAndKeyAsync(
                    userId,
                    idempotencyKey,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingRecord);

            var service = new CreateOrderService(
                productRepository.Object,
                orderRepository.Object,
                transaction.Object,
                currentUserService.Object,
                stockMovementRepository.Object,
                idempotencyRepository.Object);

            // Act
            var result = await service.CreateAsync(
                request,
                idempotencyKey,
                CancellationToken.None);

            // Assert
            Assert.Equal(201, result.StatusCode);
            Assert.Equal(savedResponse, result.ResponseBody);
            Assert.True(result.IsReplay);

            productRepository.Verify(
                x => x.GetByIdsAsync(
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            orderRepository.Verify(
                x => x.AddAsync(
                    It.IsAny<Order>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            transaction.Verify(
                x => x.ExecuteAsync(
                    It.IsAny<Func<CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }


        [Fact]
        public async Task CreateAsync_SameKeyWithDifferentRequest_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            const string idempotencyKey = "same-key-different-request";

            var originalRequest = new CreateOrderRequest(
                new[]
                {
            new CreateOrderItemRequest(Guid.NewGuid(), 2)
                });

            var differentRequest = new CreateOrderRequest(
                new[]
                {
            new CreateOrderItemRequest(Guid.NewGuid(), 3)
                });

            var now = DateTime.UtcNow;

            var existingRecord = new IdempotencyRecord(
                userId,
                idempotencyKey,
                RequestHashHelper.Compute(originalRequest),
                201,
                """{"orderId":"saved-order"}""",
                now,
                now.AddHours(24));

            var idempotencyRepository = new Mock<IIdempotencyRepository>();

            idempotencyRepository
                .Setup(x => x.GetByUserAndKeyAsync(
                    userId,
                    idempotencyKey,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingRecord);

            var currentUserService = new Mock<ICurrentUserService>();
            currentUserService
                .Setup(x => x.UserId)
                .Returns(userId);

            var productRepository = new Mock<IProductRepository>();
            var orderRepository = new Mock<IOrderRepository>();
            var transaction = new Mock<IOrderTransaction>();
            var stockMovementRepository = new Mock<IStockMovementRepository>();

            var service = new CreateOrderService(
                productRepository.Object,
                orderRepository.Object,
                transaction.Object,
                currentUserService.Object,
                stockMovementRepository.Object,
                idempotencyRepository.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(
                    differentRequest,
                    idempotencyKey,
                    CancellationToken.None));

            Assert.Contains("different request", exception.Message);

            productRepository.Verify(
                x => x.GetByIdsAsync(
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            orderRepository.Verify(
                x => x.AddAsync(
                    It.IsAny<Order>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            transaction.Verify(
                x => x.ExecuteAsync(
                    It.IsAny<Func<CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

    }
}
