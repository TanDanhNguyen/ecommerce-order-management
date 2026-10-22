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
        private readonly GetOrdersService _service;

        public GetOrdersServiceTests()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();

            _service = new GetOrdersService(
                _orderRepositoryMock.Object);
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
                    1,
                    10,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    ((IReadOnlyList<OrderListItemDto>)items, 25));

            // Act
            var result = await _service.GetPagedAsync(
                1, 10, CancellationToken.None);

            // Assert
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(25, result.TotalCount);
            Assert.Equal(3, result.TotalPages);
            Assert.Single(result.Items);
            Assert.Equal(300m, result.Items[0].TotalAmount);
            Assert.Equal(3, result.Items[0].ItemCount);
        }

        [Fact]
        public async Task GetPagedAsync_InvalidPageNumber_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.GetPagedAsync(
                    0, 10, CancellationToken.None));

            _orderRepositoryMock.Verify(
                x => x.GetPagedAsync(
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
                    1, pageSize, CancellationToken.None));

            _orderRepositoryMock.Verify(
                x => x.GetPagedAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
