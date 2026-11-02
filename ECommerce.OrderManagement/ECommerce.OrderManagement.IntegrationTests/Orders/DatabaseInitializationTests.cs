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

            var businessCanConnect =
                await database.DbContext.Database.CanConnectAsync();

            var identityCanConnect =
                await database.IdentityDbContext.Database.CanConnectAsync();

            Assert.True(businessCanConnect,
                "Cannot connect to business integration test database.");

            Assert.True(identityCanConnect,
                "Cannot connect to identity integration test database.");
        }
    }
}
