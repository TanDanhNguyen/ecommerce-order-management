using ECommerce.OrderManagement.Application.Authentication.Interfaces;
using ECommerce.OrderManagement.Application.Interfaces;
using ECommerce.OrderManagement.Application.Orders;
using ECommerce.OrderManagement.Application.Orders.DTOs;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.UnitTests.Orders
{

    public class GetOrdersServiceTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly GetOrdersService _service;
        private readonly Guid _userId;

        public GetOrdersServiceTests()
        {
            _userId = Guid.NewGuid();

            _orderRepositoryMock = new Mock<IOrderRepository>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();

            _currentUserServiceMock
                .Setup(x => x.UserId)
                .Returns(_userId);

            _service = new GetOrdersService(
                _orderRepositoryMock.Object,
                _currentUserServiceMock.Object);
        }

        [Fact]
        public async Task GetPagedAsync_ValidRequest_ReturnsPagedResult()
        {
            // Arrange
            var items = new List<OrderListItemDto>
        {
            new(
                Guid.NewGuid(),
                "Pending",
                DateTime.UtcNow,
                300m,
                3)
        };

            _orderRepositoryMock
                .Setup(x => x.GetPagedAsync(
                    _userId,
                    1,
                    10,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    ((IReadOnlyList<OrderListItemDto>)items, 25));

            // Act
            var result = await _service.GetPagedAsync(
                1,
                10,
                CancellationToken.None);

            // Assert
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(25, result.TotalCount);
            Assert.Equal(3, result.TotalPages);
            Assert.Single(result.Items);
            Assert.Equal(300m, result.Items[0].TotalAmount);
            Assert.Equal(3, result.Items[0].ItemCount);

            _orderRepositoryMock.Verify(
                x => x.GetPagedAsync(
                    _userId,
                    1,
                    10,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetPagedAsync_InvalidPageNumber_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.GetPagedAsync(
                    0,
                    10,
                    CancellationToken.None));

            _orderRepositoryMock.Verify(
                x => x.GetPagedAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(101)]
        public async Task GetPagedAsync_InvalidPageSize_ThrowsArgumentException(
            int pageSize)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.GetPagedAsync(
                    1,
                    pageSize,
                    CancellationToken.None));

            _orderRepositoryMock.Verify(
                x => x.GetPagedAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task GetPagedAsync_UserNotAuthenticated_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            _currentUserServiceMock
                .Setup(x => x.UserId)
                .Returns((Guid?)null);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.GetPagedAsync(
                    1,
                    10,
                    CancellationToken.None));

            _orderRepositoryMock.Verify(
                x => x.GetPagedAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
