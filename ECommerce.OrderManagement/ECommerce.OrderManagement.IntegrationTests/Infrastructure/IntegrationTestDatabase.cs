using ECommerce.OrderManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.IntegrationTests.Infrastructure
{
    public sealed class IntegrationTestDatabase : IAsyncDisposable
    {
        public ECommerceDbContext DbContext { get; }

        public IntegrationTestDatabase()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var connectionString = configuration.GetConnectionString(
                "IntegrationTestConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "IntegrationTestConnection is not configured.");
            }

            var options = new DbContextOptionsBuilder<ECommerceDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            DbContext = new ECommerceDbContext(options);
        }

        public async Task InitializeAsync()
        {
            await DbContext.Database.MigrateAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
        }
    }
}
