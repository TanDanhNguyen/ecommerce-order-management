using ECommerce.OrderManagement.Domain.Entities;
using ECommerce.OrderManagement.Infrastructure.Repositories;
using ECommerce.OrderManagement.IntegrationTests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.IntegrationTests.Orders
{
    [Collection("Integration Tests")]
    public class OrderRepositoryIntegrationTests
    {
        [Fact]
        public async Task GetPagedAsync_ReturnsOnlyOrdersForSpecifiedUser()
        {
            // Arrange
            await using var database = new IntegrationTestDatabase();
            await database.InitializeAsync();

            var dbContext = database.DbContext;

            dbContext.OrderItems.RemoveRange(dbContext.OrderItems);
            dbContext.Orders.RemoveRange(dbContext.Orders);
            await dbContext.SaveChangesAsync();

            var userA = Guid.NewGuid();
            var userB = Guid.NewGuid();

            var orderA = Order.Create(userA);
            var orderB = Order.Create(userB);

            dbContext.Orders.AddRange(orderA, orderB);
            await dbContext.SaveChangesAsync();

            var repository = new OrderRepository(dbContext);

            // Act
            var (items, totalCount) = await repository.GetPagedAsync(
                userA,
                1,
                10,
                CancellationToken.None);

            // Assert
            Assert.Equal(1, totalCount);
            var item = Assert.Single(items);
            Assert.Equal(orderA.Id, item.Id);
            Assert.DoesNotContain(items, x => x.Id == orderB.Id);
        }

        [Fact]
        public async Task GetPagedAsync_CountsAndPaginatesOnlyOrdersForSpecifiedUser()
        {
            // Arrange
            await using var database = new IntegrationTestDatabase();
            await database.InitializeAsync();

            var dbContext = database.DbContext;

            dbContext.OrderItems.RemoveRange(dbContext.OrderItems);
            dbContext.Orders.RemoveRange(dbContext.Orders);
            await dbContext.SaveChangesAsync();

            var userA = Guid.NewGuid();
            var userB = Guid.NewGuid();

            var userAOrders = Enumerable.Range(0, 3)
                .Select(_ => Order.Create(userA))
                .ToList();

            var userBOrders = Enumerable.Range(0, 2)
                .Select(_ => Order.Create(userB))
                .ToList();

            dbContext.Orders.AddRange(userAOrders);
            dbContext.Orders.AddRange(userBOrders);
            await dbContext.SaveChangesAsync();

            var repository = new OrderRepository(dbContext);

            // Act
            var (items, totalCount) = await repository.GetPagedAsync(
                userA,
                1,
                2,
                CancellationToken.None);

            // Assert
            Assert.Equal(3, totalCount);
            Assert.Equal(2, items.Count);
            Assert.All(
                items,
                item => Assert.Contains(
                    userAOrders,
                    order => order.Id == item.Id));
        }
    }
}
