using ECommerce.OrderManagement.Application.Interfaces;
using ECommerce.OrderManagement.Infrastructure.Persistence;
using ECommerce.OrderManagement.IntegrationTests;
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
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddJsonFile(
                    Path.Combine(
                        AppContext.BaseDirectory,
                        "appsettings.json"),
                    optional: false);
            });

            builder.ConfigureServices(services =>
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false)
                    .Build();

                var connectionString =
                    configuration.GetConnectionString(
                        "IntegrationTestConnection");

                services.RemoveAll<DbContextOptions<ECommerceDbContext>>();

                services.AddDbContext<ECommerceDbContext>(options =>
                    options.UseSqlServer(connectionString));
            });
        }
    }
}