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
    public class CancelOrderServiceTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
        private readonly Mock<IProductRepository> _productRepositoryMock = new();
        private readonly Mock<IOrderTransaction> _transactionMock = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();

        private CancelOrderService CreateService()
        {

            return new CancelOrderService(
                _orderRepositoryMock.Object,
                _productRepositoryMock.Object,
                _transactionMock.Object,
                _currentUser.Object);
        }

        [Fact]
        public async Task CancelAsync_ValidOrder_CancelsOrderAndRestoresStock()
        {
            // Arrange
            var product = new Product("Keyboard", "KB-001", 100m, 7);
            var userId = Guid.NewGuid();
            var order = Order.Create(userId);
            _currentUser
                .Setup(x => x.UserId)
                .Returns(userId);

            order.AddItem(new OrderItem(
                product.Id, "Keyboard", "KB-001", 100m, 3));

            _orderRepositoryMock
                .Setup(x => x.GetByIdAsync(
                    order.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            _productRepositoryMock
                .Setup(x => x.GetByIdsAsync(
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { product });

            _transactionMock
                .Setup(x => x.ExecuteAsync(
                    It.IsAny<Func<CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()))
                .Returns((
                    Func<CancellationToken, Task> operation,
                    CancellationToken ct) => operation(ct));

            var service = CreateService();

            // Act
            await service.CancelAsync(order.Id, CancellationToken.None);

            // Assert
            Assert.Equal(OrderStatus.Cancelled, order.Status);
            Assert.Equal(10, product.Stock);
        }

        [Fact]
        public async Task CancelAsync_OrderNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var orderId = Guid.NewGuid();

            _orderRepositoryMock
                .Setup(x => x.GetByIdAsync(
                    orderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Order?)null);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.CancelAsync(orderId, CancellationToken.None));

            _transactionMock.Verify(
                x => x.ExecuteAsync(
                    It.IsAny<Func<CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CancelAsync_EmptyOrderId_ThrowsArgumentException()
        {
            var service = CreateService();

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.CancelAsync(Guid.Empty, CancellationToken.None));
        }

        [Fact]
        public async Task CancelAsync_AlreadyCancelledOrder_ThrowsInvalidOperationException()
        {
            // Arrange
            var product = new Product("Keyboard", "KB-003", 100m, 7);
            var userId = Guid.NewGuid();
            var order = Order.Create(userId);

            _currentUser
                .Setup(x => x.UserId)
                .Returns(userId);

            order.AddItem(new OrderItem(
                product.Id, "Keyboard", "KB-003", 100m, 3));

            order.Cancel();

            _orderRepositoryMock
                .Setup(x => x.GetByIdAsync(
                    order.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CancelAsync(order.Id, CancellationToken.None));

            Assert.Equal(OrderStatus.Cancelled, order.Status);
            Assert.Equal(7, product.Stock);

            _transactionMock.Verify(
                x => x.ExecuteAsync(
                    It.IsAny<Func<CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CancelAsync_CompletedOrder_ThrowsInvalidOperationException()
        {
            // Arrange
            var product = new Product("Keyboard", "KB-004", 100m, 7);
            var userId = Guid.NewGuid();
            var order = Order.Create(userId);

            _currentUser
                .Setup(x => x.UserId)
                .Returns(userId);

            order.AddItem(new OrderItem(
                product.Id, "Keyboard", "KB-004", 100m, 3));

            order.Confirm();
            order.Complete();

            _orderRepositoryMock
                .Setup(x => x.GetByIdAsync(
                    order.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CancelAsync(order.Id, CancellationToken.None));

            Assert.Equal(OrderStatus.Completed, order.Status);
            Assert.Equal(7, product.Stock);

            _transactionMock.Verify(
                x => x.ExecuteAsync(
                    It.IsAny<Func<CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CancelAsync_TransactionFails_PropagatesException()
        {
            // Arrange
            var product = new Product("Keyboard", "KB-005", 100m, 7);
            var userId = Guid.NewGuid();
            var order = Order.Create(userId);

            _currentUser
                .Setup(x => x.UserId)
                .Returns(userId);

            order.AddItem(new OrderItem(
                product.Id, "Keyboard", "KB-005", 100m, 3));

            _orderRepositoryMock
                .Setup(x => x.GetByIdAsync(
                    order.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            _productRepositoryMock
                .Setup(x => x.GetByIdsAsync(
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<Product>)new[] { product });

            _transactionMock
                .Setup(x => x.ExecuteAsync(
                    It.IsAny<Func<CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database failure"));

            var service = CreateService();

            // Act
            var exception = await Record.ExceptionAsync(
                () => service.CancelAsync(
                    order.Id,
                    CancellationToken.None));

            // Assert
            Assert.NotNull(exception);
            Assert.Equal("Database failure", exception.Message);

            _transactionMock.Verify(
                x => x.ExecuteAsync(
                    It.IsAny<Func<CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            Assert.Equal(7, product.Stock);
            Assert.Equal(OrderStatus.Pending, order.Status);
        }

        [Fact]
        public async Task CancelAsync_OrderBelongsToAnotherUser_ThrowsKeyNotFoundException()
        {
            // Arrange
            var product = new Product("Keyboard", "KB-001", 100m, 7);

            var ownerUserId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();

            var order = Order.Create(ownerUserId);

            order.AddItem(new OrderItem(
                product.Id, "Keyboard", "KB-001", 100m, 3));

            _currentUser
                .Setup(x => x.UserId)
                .Returns(currentUserId);

            _orderRepositoryMock
                .Setup(x => x.GetByIdAsync(
                    order.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.CancelAsync(
                    order.Id,
                    CancellationToken.None));

            // Verify order and stock were not modified
            Assert.Equal(OrderStatus.Pending, order.Status);
            Assert.Equal(7, product.Stock);

            _productRepositoryMock.Verify(
                x => x.GetByIdsAsync(
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _transactionMock.Verify(
                x => x.ExecuteAsync(
                    It.IsAny<Func<CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }


        [Fact]
        public async Task CancelAsync_UserNotAuthenticated_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var ownerUserId = Guid.NewGuid();
            var order = Order.Create(ownerUserId);

            _currentUser
                .Setup(x => x.UserId)
                .Returns((Guid?)null);

            _orderRepositoryMock
                .Setup(x => x.GetByIdAsync(
                    order.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.CancelAsync(
                    order.Id,
                    CancellationToken.None));

            Assert.Equal(OrderStatus.Pending, order.Status);

            _productRepositoryMock.Verify(
                x => x.GetByIdsAsync(
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _transactionMock.Verify(
                x => x.ExecuteAsync(
                    It.IsAny<Func<CancellationToken, Task>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
