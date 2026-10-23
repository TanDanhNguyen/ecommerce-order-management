using ECommerce.OrderManagement.Application.Interfaces;
using ECommerce.OrderManagement.Application.Products;
using ECommerce.OrderManagement.Domain.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.UnitTests.Products
{
    public class GetProductsServiceTests
    {
        private readonly Mock<IProductRepository> _repositoryMock;
        private readonly GetProductsService _service;

        public GetProductsServiceTests()
        {
            _repositoryMock = new Mock<IProductRepository>();
            _service = new GetProductsService(_repositoryMock.Object);
        }

        [Fact]
        public async Task GetPagedAsync_ShouldReturnPagedProducts()
        {
            // Arrange
            var products = new List<Product>
        {
            new("Keyboard", "KB-001", 500m, 10),
            new("Mouse", "MS-001", 200m, 20)
        };


            _repositoryMock
                .Setup(x => x.GetPagedAsync(
                    1,
                    10,
                    It.IsAny<string?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(((IReadOnlyList<Product>)products, 25));

            // Act
            var result = await _service.GetPagedAsync(
                1,
                10,
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                It.IsAny<string?>(),
                It.IsAny<bool>(),
                CancellationToken.None);

            // Assert
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(25, result.TotalCount);
            Assert.Equal(2, result.Items.Count);

            Assert.Equal("Keyboard", result.Items[0].Name);
            Assert.Equal("Mouse", result.Items[1].Name);


            _repositoryMock
                .Setup(x => x.GetPagedAsync(
                    1,
                    10,
                    It.IsAny<string?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(((IReadOnlyList<Product>)products, 25));

        }

        [Theory]
        [InlineData(0, 10)]
        [InlineData(-1, 10)]
        [InlineData(1, 0)]
        [InlineData(1, -1)]
        [InlineData(1, 101)]
        public async Task GetPagedAsync_ShouldThrow_WhenPaginationIsInvalid(
            int pageNumber,
            int pageSize)
        {

            // Act
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => _service.GetPagedAsync(
                    pageNumber,
                    pageSize,
                    search: null,
                    isActive: null,
                    sortBy: null,
                    sortDescending: false,
                    cancellationToken: CancellationToken.None));

            // Assert
            _repositoryMock.Verify(
                x => x.GetPagedAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

        }
    }
}
