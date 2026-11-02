using ECommerce.OrderManagement.Infrastructure.Identity;
using ECommerce.OrderManagement.Infrastructure.Persistence;
using ECommerce.OrderManagement.IntegrationTests.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace ECommerce.OrderManagement.Infrastructure.Infrastructure
{
    public class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            var businessConnectionString =
                configuration["IntegrationTestConnection"]
                ?? configuration.GetConnectionString("IntegrationTestConnection")
                ?? throw new InvalidOperationException(
                    "IntegrationTestConnection is missing.");

            var identityConnectionString =
                configuration["IdentityIntegrationTestConnection"]
                ?? configuration.GetConnectionString("IdentityIntegrationTestConnection")
                ?? throw new InvalidOperationException(
                    "IdentityIntegrationTestConnection is missing.");

            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] =
                            businessConnectionString,

                        ["IdentityIntegrationTestConnection"] =
                            identityConnectionString,

                        ["ConnectionStrings:IdentityIntegrationTestConnection"] =
                            identityConnectionString
                    });
            });

            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication()
                    .AddScheme<
                        AuthenticationSchemeOptions,
                        TestAuthHandler>(
                        TestAuthHandler.SchemeName,
                        _ => { });
            });
        }

        public async Task VerifyTestDatabasesAsync()
        {
            using var scope = Services.CreateScope();

            var dbContext = scope.ServiceProvider
                .GetRequiredService<ECommerceDbContext>();

            var identityDbContext = scope.ServiceProvider
                .GetRequiredService<ApplicationIdentityDbContext>();

            var businessDatabase =
                dbContext.Database.GetDbConnection().Database;

            var identityDatabase =
                identityDbContext.Database.GetDbConnection().Database;

            if (string.IsNullOrWhiteSpace(businessDatabase) ||
                !businessDatabase.Contains(
                    "IntegrationTests",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"ECommerceDbContext is connected to '{businessDatabase}', " +
                    "which is not a clearly named test database.");
            }

            if (string.IsNullOrWhiteSpace(identityDatabase) ||
                !identityDatabase.Contains(
                    "IntegrationTests",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"ApplicationIdentityDbContext is connected to '{identityDatabase}', " +
                    "which is not a clearly named test database.");
            }

            if (string.Equals(
                businessDatabase,
                identityDatabase,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Business and Identity test databases must be separate.");
            }

            await Task.CompletedTask;
        }

        public async Task InitializeDatabasesAsync()
        {
            using var scope = Services.CreateScope();

            var dbContext = scope.ServiceProvider
                .GetRequiredService<ECommerceDbContext>();

            var identityDbContext = scope.ServiceProvider
                .GetRequiredService<ApplicationIdentityDbContext>();

            await dbContext.Database.MigrateAsync();
            await identityDbContext.Database.MigrateAsync();
        }
    }
}
