using ECommerce.OrderManagement.Application.Authentication.Interfaces;
using ECommerce.OrderManagement.Application.Interfaces;
using ECommerce.OrderManagement.Application.Orders;
using ECommerce.OrderManagement.Domain.Entities;
using ECommerce.OrderManagement.Domain.Enums;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.UnitTests.Orders
{
    public class GetOrderByIdServiceTests
    {
        [Fact]
        public async Task GetByIdAsync_OrderExists_ReturnsOrderDto()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var order = Order.Create(userId);

            var product = new Product(
                "Keyboard",
                "KB-001",
                100m,
                10);

            order.AddItem(new OrderItem(
                product.Id,
                product.Name,
                product.Sku,
                product.Price,
                2));

            var repository = new Mock<IOrderRepository>();
            var currentUserServiceMock = new Mock<ICurrentUserService>();

            currentUserServiceMock
                .Setup(x => x.UserId)
                .Returns(userId);

            repository
                .Setup(x => x.GetByIdAsync(
                    order.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            var service = new GetOrderByIdService(repository.Object, currentUserServiceMock.Object);

            // Act
            var result = await service.GetByIdAsync(
                order.Id,
                CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(order.Id, result.Id);
            Assert.Equal(OrderStatus.Pending.ToString(), result.Status);
            Assert.Equal(200m, result.TotalAmount);
            Assert.Single(result.Items);
            Assert.Equal("Keyboard", result.Items[0].ProductName);
            Assert.Equal(2, result.Items[0].Quantity);
        }

        [Fact]
        public async Task GetByIdAsync_OrderDoesNotExist_ReturnsNull()
        {
            // Arrange
            var repository = new Mock<IOrderRepository>();
            var currentUserServiceMock = new Mock<ICurrentUserService>();

            repository
                .Setup(x => x.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Order?)null);

            var service = new GetOrderByIdService(repository.Object, currentUserServiceMock.Object);

            // Act
            var result = await service.GetByIdAsync(
                Guid.NewGuid(),
                CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_EmptyGuid_ThrowsArgumentException()
        {
            // Arrange
            var repository = new Mock<IOrderRepository>();
            var currentUserServiceMock = new Mock<ICurrentUserService>();
            var service = new GetOrderByIdService(repository.Object, currentUserServiceMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.GetByIdAsync(
                    Guid.Empty,
                    CancellationToken.None));

            repository.Verify(
                x => x.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_OrderBelongsToAnotherUser_ReturnsNull()
        {
            // Arrange
            var orderOwnerId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();

            var order = Order.Create(orderOwnerId);

            var repository = new Mock<IOrderRepository>();
            repository
                .Setup(x => x.GetByIdAsync(
                    order.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            var currentUserServiceMock = new Mock<ICurrentUserService>();
            currentUserServiceMock
                .Setup(x => x.UserId)
                .Returns(currentUserId);

            var service = new GetOrderByIdService(
                repository.Object,
                currentUserServiceMock.Object);

            // Act
            var result = await service.GetByIdAsync(
                order.Id,
                CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_UserNotAuthenticated_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var order = Order.Create(userId);

            var repository = new Mock<IOrderRepository>();
            repository
                .Setup(x => x.GetByIdAsync(
                    order.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            var currentUserServiceMock = new Mock<ICurrentUserService>();
            currentUserServiceMock
                .Setup(x => x.UserId)
                .Returns((Guid?)null);

            var service = new GetOrderByIdService(
                repository.Object,
                currentUserServiceMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.GetByIdAsync(
                    order.Id,
                    CancellationToken.None));
        }
    }
}
