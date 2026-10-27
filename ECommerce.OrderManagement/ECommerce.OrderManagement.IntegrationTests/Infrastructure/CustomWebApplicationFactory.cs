using ECommerce.OrderManagement.Application.Interfaces;
using ECommerce.OrderManagement.Infrastructure.Identity;
using ECommerce.OrderManagement.Infrastructure.Persistence;
using ECommerce.OrderManagement.IntegrationTests;
using ECommerce.OrderManagement.IntegrationTests.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;


namespace ECommerce.OrderManagement.Infrastructure.Infrastructure
{
    public class CustomWebApplicationFactory
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false)
                    .AddEnvironmentVariables()
                    .Build();

                var connectionString =
                    configuration["IntegrationTestConnection"]
                    ?? configuration.GetConnectionString("IntegrationTestConnection")
                    ?? throw new InvalidOperationException(
                        "IntegrationTestConnection is missing.");

                var identityConnectionString =
                    configuration["IdentityIntegrationTestConnection"]
                    ?? configuration.GetConnectionString("IdentityIntegrationTestConnection")
                    ?? throw new InvalidOperationException(
                        "IdentityIntegrationTestConnection is missing.");

                services.RemoveAll<
                    DbContextOptions<ECommerceDbContext>>();

                services.RemoveAll<
                    DbContextOptions<ApplicationIdentityDbContext>>();

                services.AddDbContext<ECommerceDbContext>(options =>
                    options.UseSqlServer(connectionString));

                services.AddDbContext<ApplicationIdentityDbContext>(options =>
                    options.UseSqlServer(identityConnectionString));
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

            if (businessDatabase == identityDatabase)
            {
                throw new InvalidOperationException(
                    "Business and Identity test databases must be separate.");
            }

            if (!businessDatabase.Contains(
                    "IntegrationTests",
                    StringComparison.OrdinalIgnoreCase) ||
                !identityDatabase.Contains(
                    "IntegrationTests",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "A DbContext is not connected to a clearly named test database.");
            }
        }
    }
}