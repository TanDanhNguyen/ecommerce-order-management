using ECommerce.OrderManagement.Application.Authentication.Interfaces;
using ECommerce.OrderManagement.Application.Exceptions;
using ECommerce.OrderManagement.Application.Interfaces;
using ECommerce.OrderManagement.Application.Orders;
using ECommerce.OrderManagement.Application.Requests;
using ECommerce.OrderManagement.Domain.Entities;
using ECommerce.OrderManagement.Domain.Enums;
using Moq;

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

            var service = new CreateOrderService(
                productRepository.Object,
                orderRepository.Object,
                transaction.Object,
                currentUserService.Object);

            var request = new CreateOrderRequest(
                new[]
                {
                new CreateOrderItemRequest(product.Id, 2)
                });

            // Act
            var order = await service.CreateAsync(
                request,
                CancellationToken.None);

            // Assert
            Assert.Equal(OrderStatus.Pending, order.Status);
            Assert.Single(order.Items);
            Assert.Equal(200m, order.TotalAmount);
            Assert.Equal(8, product.Stock);

            orderRepository.Verify(
                x => x.AddAsync(
                    It.Is<Order>(o => o.Id == order.Id),
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

            var service = new CreateOrderService(
                productRepository.Object,
                orderRepository.Object,
                transaction.Object,
                currentUserService.Object);

            var request = new CreateOrderRequest(
                new[]
                {
            new CreateOrderItemRequest(product.Id, 3)
                });

            // Act & Assert
            await Assert.ThrowsAsync<InsufficientStockException>(
                () => service.CreateAsync(request, CancellationToken.None));

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

            var service = new CreateOrderService(
                productRepository.Object,
                orderRepository.Object,
                transaction.Object,
                currentUserService.Object);

            var request = new CreateOrderRequest(
                new[]
                {
            new CreateOrderItemRequest(Guid.NewGuid(), 1)
                });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(request, CancellationToken.None));

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

            var service = new CreateOrderService(
                productRepository.Object,
                orderRepository.Object,
                transaction.Object,
                currentUserService.Object);

            var request = new CreateOrderRequest(
                Array.Empty<CreateOrderItemRequest>());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.CreateAsync(request, CancellationToken.None));

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

            var service = new CreateOrderService(
                productRepository.Object,
                orderRepository.Object,
                transaction.Object,
                currentUserService.Object);

            var request = new CreateOrderRequest(
                new[]
                {
            new CreateOrderItemRequest(product.Id, 0)
                });

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.CreateAsync(request, CancellationToken.None));

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

            // Arrange
            var service = new CreateOrderService(
                CreateProductRepository().Object,
                new Mock<IOrderRepository>().Object,
                new Mock<IOrderTransaction>().Object,
                currentUserService.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => service.CreateAsync(
                    null!,
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
    }
}
