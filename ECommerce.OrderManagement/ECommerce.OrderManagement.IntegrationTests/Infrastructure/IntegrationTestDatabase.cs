using ECommerce.OrderManagement.Infrastructure.Identity;
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

        public ApplicationIdentityDbContext IdentityDbContext { get; }

        public IntegrationTestDatabase()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration["IntegrationTestConnection"]
                ?? configuration.GetConnectionString("IntegrationTestConnection")
                ?? throw new InvalidOperationException(
                    "IntegrationTestConnection is not configured.");

            var identityConnectionString =
                configuration["IdentityIntegrationTestConnection"]
                ?? configuration.GetConnectionString(
                    "IdentityIntegrationTestConnection")
                ?? throw new InvalidOperationException(
                    "IdentityIntegrationTestConnection is not configured.");

            var dbOptions = new DbContextOptionsBuilder<ECommerceDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            var identityOptions =
                new DbContextOptionsBuilder<ApplicationIdentityDbContext>()
                    .UseSqlServer(identityConnectionString)
                    .Options;

            DbContext = new ECommerceDbContext(dbOptions);
            IdentityDbContext =
                new ApplicationIdentityDbContext(identityOptions);
        }

        public async Task InitializeAsync()
        {
            await DbContext.Database.MigrateAsync();
            await IdentityDbContext.Database.MigrateAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await IdentityDbContext.DisposeAsync();
        }
    }
}

