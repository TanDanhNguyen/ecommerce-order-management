using ECommerce.OrderManagement.Application.Exceptions;
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
    public class UpdateProductServiceTests
    {
        private readonly Mock<IProductRepository> _repositoryMock;
        private readonly UpdateProductService _service;

        public UpdateProductServiceTests()
        {
            _repositoryMock = new Mock<IProductRepository>();
            _service = new UpdateProductService(_repositoryMock.Object);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateProduct_WhenDataIsValid()
        {
            // Arrange
            var product = new Product("Keyboard", "KB-001", 500m, 10);

            _repositoryMock
                .Setup(x => x.GetByIdAsync(
                    product.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            _repositoryMock
                .Setup(x => x.ExistsBySkuAsync(
                    "KB-002",
                    product.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _repositoryMock
                .Setup(x => x.UpdateAsync(
                    It.IsAny<Product>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateAsync(
                product.Id,
                "Mechanical Keyboard",
                " KB-002 ",
                750m,
                15,
                CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.Equal("Mechanical Keyboard", product.Name);
            Assert.Equal("KB-002", product.Sku);
            Assert.Equal(750m, product.Price);
            Assert.Equal(15, product.Stock);

            _repositoryMock.Verify(
                x => x.UpdateAsync(
                    product,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenProductDoesNotExist()
        {
            // Arrange
            var productId = Guid.NewGuid();

            _repositoryMock
                .Setup(x => x.GetByIdAsync(
                    productId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await _service.UpdateAsync(
                productId,
                "Keyboard",
                "KB-002",
                750m,
                15,
                CancellationToken.None);

            // Assert
            Assert.False(result);

            _repositoryMock.Verify(
                x => x.ExistsBySkuAsync(
                    It.IsAny<string>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _repositoryMock.Verify(
                x => x.UpdateAsync(
                    It.IsAny<Product>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowDuplicateProductSkuException_WhenSkuExists()
        {
            // Arrange
            var product = new Product("Keyboard", "KB-001", 500m, 10);

            _repositoryMock
                .Setup(x => x.GetByIdAsync(
                    product.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            _repositoryMock
                .Setup(x => x.ExistsBySkuAsync(
                    "KB-002",
                    product.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            await Assert.ThrowsAsync<DuplicateProductSkuException>(
                () => _service.UpdateAsync(
                    product.Id,
                    "Keyboard",
                    "KB-002",
                    750m,
                    15,
                    CancellationToken.None));

            // Assert
            _repositoryMock.Verify(
                x => x.UpdateAsync(
                    It.IsAny<Product>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowArgumentOutOfRangeException_WhenPriceIsZero()
        {
            // Arrange
            var product = new Product("Keyboard", "KB-001", 500m, 10);

            _repositoryMock
                .Setup(x => x.GetByIdAsync(
                    product.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            _repositoryMock
                .Setup(x => x.ExistsBySkuAsync(
                    "KB-001",
                    product.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => _service.UpdateAsync(
                    product.Id,
                    "Keyboard",
                    "KB-001",
                    0m,
                    10,
                    CancellationToken.None));

            // Assert
            _repositoryMock.Verify(
                x => x.UpdateAsync(
                    It.IsAny<Product>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowArgumentOutOfRangeException_WhenStockIsNegative()
        {
            // Arrange
            var product = new Product("Keyboard", "KB-001", 500m, 10);

            _repositoryMock
                .Setup(x => x.GetByIdAsync(
                    product.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            _repositoryMock
                .Setup(x => x.ExistsBySkuAsync(
                    "KB-001",
                    product.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => _service.UpdateAsync(
                    product.Id,
                    "Keyboard",
                    "KB-001",
                    750m,
                    -1,
                    CancellationToken.None));

            // Assert
            _repositoryMock.Verify(
                x => x.UpdateAsync(
                    It.IsAny<Product>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}