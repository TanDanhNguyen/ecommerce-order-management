using ECommerce.OrderManagement.Application.Interfaces;
using ECommerce.OrderManagement.Application.StockMovements;
using ECommerce.OrderManagement.Domain.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.UnitTests.Stock
{
    public class GetStockMovementsServiceTests
    {
        [Fact]
        public async Task ExecuteAsync_InvalidOperation_ThrowsArgumentException()
        {
            // Arrange
            var stockMovementRepository =
                new Mock<IStockMovementRepository>();

            var productRepository =
                new Mock<IProductRepository>();

            var service = new GetStockMovementsService(
                stockMovementRepository.Object,
                productRepository.Object);

            var productId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.ExecuteAsync(
                    productId,
                    pageNumber: 1,
                    pageSize: 10,
                    operation: "ABC",
                    cancellationToken: CancellationToken.None));
        }
        [Fact]
        public async Task ExecuteAsync_ValidOperation_ReturnsFilteredMovements()
        {
            // Arrange
            var productId = Guid.NewGuid();

            var product = new Product(
                "Keyboard",
                "KB-001",
                100m,
                10);

            var stockMovementRepository =
                new Mock<IStockMovementRepository>();

            var productRepository =
                new Mock<IProductRepository>();

            productRepository
                .Setup(x => x.GetByIdAsync(
                    productId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            stockMovementRepository
                .Setup(x => x.GetByProductIdAsync(
                    productId,
                    1,
                    10,
                    "Order",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StockMovement>());

            stockMovementRepository
                .Setup(x => x.CountByProductIdAsync(
                    productId,
                    "Order",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var service = new GetStockMovementsService(
                stockMovementRepository.Object,
                productRepository.Object);

            // Act
            var result = await service.ExecuteAsync(
                productId,
                1,
                10,
                "Order",
                CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalCount);

            stockMovementRepository.Verify(
                x => x.GetByProductIdAsync(
                    productId,
                    1,
                    10,
                    "Order",
                    It.IsAny<CancellationToken>()),
                Times.Once);

            stockMovementRepository.Verify(
                x => x.CountByProductIdAsync(
                    productId,
                    "Order",
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        [Fact]
        public async Task ExecuteAsync_ProductNotFound_ReturnsNull()
        {
            // Arrange
            var productId = Guid.NewGuid();

            var stockMovementRepository =
                new Mock<IStockMovementRepository>();

            var productRepository =
                new Mock<IProductRepository>();

            productRepository
                .Setup(x => x.GetByIdAsync(
                    productId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            var service = new GetStockMovementsService(
                stockMovementRepository.Object,
                productRepository.Object);

            // Act
            var result = await service.ExecuteAsync(
                productId,
                pageNumber: 1,
                pageSize: 10,
                operation: null,
                cancellationToken: CancellationToken.None);

            // Assert
            Assert.Null(result);

            stockMovementRepository.Verify(
                x => x.GetByProductIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            stockMovementRepository.Verify(
                x => x.CountByProductIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
        [Fact]
        public async Task ExecuteAsync_ReturnsCorrectPaginationMetadata()
        {
            // Arrange
            var productId = Guid.NewGuid();

            var product = new Product(
                "Keyboard",
                "KB-001",
                100m,
                10);

            var stockMovementRepository =
                new Mock<IStockMovementRepository>();

            var productRepository =
                new Mock<IProductRepository>();

            productRepository
                .Setup(x => x.GetByIdAsync(
                    productId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            stockMovementRepository
                .Setup(x => x.GetByProductIdAsync(
                    productId,
                    2,
                    2,
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StockMovement>());

            stockMovementRepository
                .Setup(x => x.CountByProductIdAsync(
                    productId,
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(5);

            var service = new GetStockMovementsService(
                stockMovementRepository.Object,
                productRepository.Object);

            // Act
            var result = await service.ExecuteAsync(
                productId,
                pageNumber: 2,
                pageSize: 2,
                operation: null,
                cancellationToken: CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.PageNumber);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(3, result.TotalPages);
            Assert.Empty(result.Items);
        }
    }
}
