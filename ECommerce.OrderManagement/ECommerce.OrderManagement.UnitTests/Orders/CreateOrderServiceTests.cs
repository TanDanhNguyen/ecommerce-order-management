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

            var service = new CreateOrderService(
                productRepository.Object,
                orderRepository.Object,
                transaction.Object);

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
    }
}
