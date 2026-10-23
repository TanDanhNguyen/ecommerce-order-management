using ECommerce.OrderManagement.Domain.Entities;
using ECommerce.OrderManagement.Infrastructure.Persistence;
using ECommerce.OrderManagement.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.IntegrationTests.Orders
{
    [Collection("Integration Tests")]
    public class ProductConcurrencyIntegrationTests
    {
        [Fact]
        public async Task SaveChanges_WhenTwoContextsUpdateSameProduct_ShouldDetectConcurrencyConflict()
        {
            await using var database = new IntegrationTestDatabase();
            await database.InitializeAsync();

            var options = new DbContextOptionsBuilder<ECommerceDbContext>()
                .UseSqlServer(
                    new ConfigurationBuilder()
                        .SetBasePath(AppContext.BaseDirectory)
                        .AddJsonFile("appsettings.json")
                        .Build()
                        .GetConnectionString("IntegrationTestConnection"))
                .Options;

            Guid productId;

            await using (var setupContext = new ECommerceDbContext(options))
            {
                setupContext.OrderItems.RemoveRange(setupContext.OrderItems);
                await setupContext.SaveChangesAsync();

                setupContext.Orders.RemoveRange(setupContext.Orders);
                await setupContext.SaveChangesAsync();

                setupContext.Products.RemoveRange(setupContext.Products);
                await setupContext.SaveChangesAsync();

                var product = new Product(
                    "Concurrency Test Product",
                    $"CC-{Guid.NewGuid():N}",
                    100m,
                    10);

                setupContext.Products.Add(product);
                try
                {
                    await setupContext.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    throw new Exception(
                        $"Failed to insert Product. " +
                        $"Inner exception: {ex.InnerException?.Message}",
                        ex);
                }

                productId = product.Id;
            }

            await using var contextA = new ECommerceDbContext(options);
            await using var contextB = new ECommerceDbContext(options);

            var productA = await contextA.Products.SingleAsync(p => p.Id == productId);
            var productB = await contextB.Products.SingleAsync(p => p.Id == productId);

            productA.SetStock(8);
            await contextA.SaveChangesAsync();

            productB.SetStock(6);

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                () => contextB.SaveChangesAsync());

            await using var verifyContext = new ECommerceDbContext(options);

            var savedProduct = await verifyContext.Products
                .AsNoTracking()
                .SingleAsync(p => p.Id == productId);

            Assert.Equal(8, savedProduct.Stock);
        }
    }
}
