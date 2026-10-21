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
    public class CreateProductServiceTests
    {
        private readonly Mock<IProductRepository> _repositoryMock;
        private readonly CreateProductService _service;

        public CreateProductServiceTests()
        {
            _repositoryMock = new Mock<IProductRepository>();

            _service = new CreateProductService(
                _repositoryMock.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateProduct_WhenSkuDoesNotExist()
        {
            // Arrange
            _repositoryMock
                .Setup(x => x.ExistsBySkuAsync(
                    "KB-001",
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _repositoryMock
                .Setup(x => x.AddAsync(
                    It.IsAny<Product>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var productId = await _service.CreateAsync(
                "Keyboard",
                " KB-001 ",
                500m,
                10,
                CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, productId);

            _repositoryMock.Verify(
                x => x.ExistsBySkuAsync(
                    "KB-001",
                    null,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _repositoryMock.Verify(
                x => x.AddAsync(
                    It.Is<Product>(p =>
                        p.Name == "Keyboard" &&
                        p.Sku == "KB-001"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowDuplicateProductSkuException_WhenSkuExists()
        {
            // Arrange
            _repositoryMock
                .Setup(x => x.ExistsBySkuAsync(
                    "KB-001",
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var exception = await Assert.ThrowsAsync<
                DuplicateProductSkuException>(
                () => _service.CreateAsync(
                    "Keyboard",
                    " KB-001 ",
                    500m,
                    10,
                    CancellationToken.None));

            // Assert
            Assert.Contains("KB-001", exception.Message);

            _repositoryMock.Verify(
                x => x.AddAsync(
                    It.IsAny<Product>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
        [Fact]
        public async Task CreateAsync_ShouldThrowArgumentException_WhenNameIsEmpty()
        {
            // Arrange
            _repositoryMock
                .Setup(x => x.ExistsBySkuAsync(
                    "KB-001",
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var exception = await Assert.ThrowsAnyAsync<ArgumentException>(
                () => _service.CreateAsync(
                    "",
                    "KB-001",
                    500m,
                    10,
                    CancellationToken.None));

            // Assert
            Assert.Contains("name", exception.Message, StringComparison.OrdinalIgnoreCase);

            _repositoryMock.Verify(
                x => x.AddAsync(
                    It.IsAny<Product>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowArgumentOutOfRangeException_WhenPriceIsZero()
        {
            // Arrange
            _repositoryMock
                .Setup(x => x.ExistsBySkuAsync(
                    "KB-001",
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => _service.CreateAsync(
                    "Keyboard",
                    "KB-001",
                    0m,
                    10,
                    CancellationToken.None));

            // Assert
            _repositoryMock.Verify(
                x => x.AddAsync(
                    It.IsAny<Product>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowArgumentOutOfRangeException_WhenStockIsNegative()
        {
            // Arrange
            _repositoryMock
                .Setup(x => x.ExistsBySkuAsync(
                    "KB-001",
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => _service.CreateAsync(
                    "Keyboard",
                    "KB-001",
                    500m,
                    -1,
                    CancellationToken.None));

            // Assert
            _repositoryMock.Verify(
                x => x.AddAsync(
                    It.IsAny<Product>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
