using ECommerce.OrderManagement.Application;
using ECommerce.OrderManagement.Application.Interfaces;
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
        public async Task AddAsync_ShouldAddStockMovement()
        {
            
        }

        [Fact]
        public async Task GetByProductIdAsync_ShouldReturnPagedStockMovements()
        {

        }
    }
}