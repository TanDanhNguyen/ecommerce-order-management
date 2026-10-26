using ECommerce.OrderManagement.Domain.Entities;
using ECommerce.OrderManagement.Infrastructure.Persistence;
using ECommerce.OrderManagement.Infrastructure.Repositories;
using ECommerce.OrderManagement.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.IntegrationTests
{
    public class IdempotencyRepositoryTests
    {
        [Fact]
        public async Task DeleteExpiredAsync_DeletesOnlyExpiredRecords()
        {
            // Arrange
            await using var database = new IntegrationTestDatabase();
            await database.InitializeAsync();

            var dbContext = database.DbContext;

            dbContext.IdempotencyRecords.RemoveRange(
                dbContext.IdempotencyRecords);

            await dbContext.SaveChangesAsync();

            var nowUtc = DateTime.UtcNow;
            var userId = Guid.NewGuid();

            var expiredRecord = new IdempotencyRecord(
                userId,
                $"expired-{Guid.NewGuid():N}",
                "hash-expired",
                201,
                """{"orderId":"expired"}""",
                nowUtc.AddHours(-25),
                nowUtc.AddHours(-1));

            var activeRecord = new IdempotencyRecord(
                userId,
                $"active-{Guid.NewGuid():N}",
                "hash-active",
                201,
                """{"orderId":"active"}""",
                nowUtc.AddHours(-1),
                nowUtc.AddHours(23));

            dbContext.IdempotencyRecords.AddRange(
                expiredRecord,
                activeRecord);

            await dbContext.SaveChangesAsync();

            var repository = new IdempotencyRepository(dbContext);

            // Act
            var deletedCount = await repository.DeleteExpiredAsync(
                nowUtc,
                CancellationToken.None);

            // Assert
            Assert.Equal(1, deletedCount);

            Assert.False(dbContext.IdempotencyRecords
                .Any(x => x.Key == expiredRecord.Key));

            Assert.True(dbContext.IdempotencyRecords
                .Any(x => x.Key == activeRecord.Key));
        }
    }
}
