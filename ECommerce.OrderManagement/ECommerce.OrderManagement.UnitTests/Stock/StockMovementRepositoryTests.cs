using ECommerce.OrderManagement.Application;
using ECommerce.OrderManagement.Application.Exceptions;
using ECommerce.OrderManagement.Application.Interfaces;
using ECommerce.OrderManagement.Application.Products;
using ECommerce.OrderManagement.Domain.Entities;
using ECommerce.OrderManagement.Infrastructure.Persistence;
using ECommerce.OrderManagement.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ECommerce.OrderManagement.UnitTests.Repositories
{
    public class StockMovementRepositoryTests
    {
        private readonly Mock<IStockMovementRepository> _stockMovementRepositoryMock;
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly AdjustProductStockService _service;

        public StockMovementRepositoryTests()
        {
            _stockMovementRepositoryMock = new Mock<IStockMovementRepository>();
            _productRepositoryMock = new Mock<IProductRepository>();

            _service = new AdjustProductStockService(
                _productRepositoryMock.Object,
                _stockMovementRepositoryMock.Object);
        }


        [Fact]
        public async Task AdjustAsync_InactiveProduct_ThrowsInactiveProductException()
        {
            // Arrange
            var product = new Product(
                "Keyboard",
                "KB-001",
                100m,
                10);

            product.Deactivate();

            _productRepositoryMock
                .Setup(x => x.GetByIdAsync(
                    product.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            // Act & Assert
            await Assert.ThrowsAsync<InactiveProductException>(
                () => _service.AdjustAsync(
                    product.Id,
                    1,
                    StockAdjustmentOperation.Increase,
                    CancellationToken.None));
        }

    }
}