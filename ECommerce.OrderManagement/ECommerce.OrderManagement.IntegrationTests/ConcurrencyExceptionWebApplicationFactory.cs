using ECommerce.OrderManagement.Application.Interfaces;
using ECommerce.OrderManagement.Infrastructure.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ECommerce.OrderManagement.IntegrationTests
{
    public sealed class ConcurrencyExceptionWebApplicationFactory
      : CustomWebApplicationFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IOrderTransaction>();

                services.AddScoped<
                    IOrderTransaction,
                    ConcurrencyExceptionOrderTransaction>();
            });
        }
    }
}
