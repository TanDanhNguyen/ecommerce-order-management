using ECommerce.OrderManagement.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.IntegrationTests.Orders
{
    [Collection("Integration Tests")]
    public class DatabaseInitializationTests
    {

        [Fact]
        public async Task InitializeAsync_CreatesDatabaseAndAppliesMigrations()
        {
            await using var database = new IntegrationTestDatabase();

            await database.InitializeAsync();

            var canConnect = await database.DbContext.Database.CanConnectAsync();

            Assert.True(
                canConnect,
                $"Cannot connect to database: " +
                $"{database.DbContext.Database.GetDbConnection().DataSource} / " +
                $"{database.DbContext.Database.GetDbConnection().Database}");
        }

    }
}
