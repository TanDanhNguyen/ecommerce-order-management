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
    public class GetProductByIdServiceTests
    {
        private readonly Mock<IProductRepository> _repositoryMock;
        private readonly GetProductByIdService _service;

        public GetProductByIdServiceTests()
        {
            _repositoryMock = new Mock<IProductRepository>();
            _service = new GetProductByIdService(_repositoryMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnProductDto_WhenProductExists()
        {
            // Arrange
            var product = new Product("Keyboard", "KB-001", 500m, 10);

            _repositoryMock
                .Setup(x => x.GetByIdAsync(
                    product.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            // Act
            var result = await _service.GetByIdAsync(
                product.Id,
                CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(product.Id, result.Id);
            Assert.Equal("Keyboard", result.Name);
            Assert.Equal("KB-001", result.Sku);
            Assert.Equal(500m, result.Price);
            Assert.Equal(10, result.Stock);
            Assert.True(result.IsActive);

            _repositoryMock.Verify(
                x => x.GetByIdAsync(
                    product.Id,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
        {
            // Arrange
            var productId = Guid.NewGuid();

            _repositoryMock
                .Setup(x => x.GetByIdAsync(
                    productId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await _service.GetByIdAsync(
                productId,
                CancellationToken.None);

            // Assert
            Assert.Null(result);

            _repositoryMock.Verify(
                x => x.GetByIdAsync(
                    productId,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
