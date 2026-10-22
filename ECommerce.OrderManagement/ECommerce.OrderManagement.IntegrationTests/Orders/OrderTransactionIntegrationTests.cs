using ECommerce.OrderManagement.Domain.Entities;
using ECommerce.OrderManagement.Infrastructure.Persistence;
using ECommerce.OrderManagement.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.IntegrationTests.Orders
{
    public class OrderTransactionIntegrationTests
    {
        [Fact]
        public async Task ExecuteAsync_WhenOperationFails_ShouldRollbackDatabaseChanges()
        {
            // Arrange
            await using var database = new IntegrationTestDatabase();
            await database.InitializeAsync();

            var db = database.DbContext;
            db.OrderItems.RemoveRange(db.OrderItems);
            db.Orders.RemoveRange(db.Orders);
            db.Products.RemoveRange(db.Products);
            await db.SaveChangesAsync();

            var product = new Product(
                "Rollback Test Product",
                $"RB-{Guid.NewGuid():N}",
                100m,
                10);

            db.Products.Add(product);
            await db.SaveChangesAsync();

            var productId = product.Id;
            var originalStock = product.Stock;

            var order = Order.Create();

            order.AddItem(new OrderItem(
                product.Id,
                product.Name,
                product.Sku,
                product.Price,
                3));

            var transaction = new OrderTransaction(
                db,
                db);

            // Act
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => transaction.ExecuteAsync(
                    async cancellationToken =>
                    {
                        product.DecreaseStock(3);
                        await db.Orders.AddAsync(order, cancellationToken);

                        await db.SaveChangesAsync(cancellationToken);

                        throw new InvalidOperationException(
                            "Simulated failure before commit.");
                    },
                    CancellationToken.None));

    
            db.ChangeTracker.Clear();

            var savedProduct = await db.Products
                .AsNoTracking()
                .SingleAsync(p => p.Id == productId);

            var orderExists = await db.Orders
                .AsNoTracking()
                .AnyAsync(o => o.Id == order.Id);

            Assert.Equal(originalStock, savedProduct.Stock);
            Assert.False(orderExists);
        }
    }
}
