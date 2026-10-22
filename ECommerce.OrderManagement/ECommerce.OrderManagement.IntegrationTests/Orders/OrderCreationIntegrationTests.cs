using ECommerce.OrderManagement.Application.Exceptions;
using ECommerce.OrderManagement.Application.Orders;
using ECommerce.OrderManagement.Application.Requests;
using ECommerce.OrderManagement.Domain.Entities;
using ECommerce.OrderManagement.Infrastructure.Persistence;
using ECommerce.OrderManagement.Infrastructure.Repositories;
using ECommerce.OrderManagement.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.OrderManagement.IntegrationTests.Orders
{
    public class OrderCreationIntegrationTests
    {
        [Fact]
        public async Task CreateOrder_WhenStockIsInsufficient_ShouldNotChangeStock()
        {
            // Arrange
            await using var database = new IntegrationTestDatabase();
            await database.InitializeAsync();

            var dbContext = database.DbContext;

            dbContext.OrderItems.RemoveRange(dbContext.OrderItems);
            dbContext.Orders.RemoveRange(dbContext.Orders);
            dbContext.Products.RemoveRange(dbContext.Products);
            await dbContext.SaveChangesAsync();

            var product = new Product(
                "Integration Test Product",
                $"IT-{Guid.NewGuid():N}",
                100m,
                5);

            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();

            var originalStock = product.Stock;

            var productRepository = new ProductRepository(dbContext);
            var orderRepository = new OrderRepository(dbContext);
            var transaction = new OrderTransaction(
                dbContext,
                dbContext);

            var service = new CreateOrderService(
                productRepository,
                orderRepository,
                transaction);

            var request = new CreateOrderRequest(
                new[]
                {
                new CreateOrderItemRequest(product.Id, 10)
                });

            // Act
            await Assert.ThrowsAsync<InsufficientStockException>(
                () => service.CreateAsync(
                    request,
                    CancellationToken.None));

            dbContext.ChangeTracker.Clear();

            var savedProduct = await dbContext.Products
                .AsNoTracking()
                .SingleAsync(p => p.Id == product.Id);

            var orderCount = await dbContext.Orders.CountAsync();

            // Assert
            Assert.Equal(originalStock, savedProduct.Stock);
            Assert.Equal(0, orderCount);
        }
    }
}
