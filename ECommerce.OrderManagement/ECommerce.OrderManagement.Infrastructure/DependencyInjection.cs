using ECommerce.OrderManagement.Application.Interfaces;
using ECommerce.OrderManagement.Infrastructure.Persistence;
using ECommerce.OrderManagement.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ECommerce.OrderManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "DefaultConnection is missing.");

            services.AddDbContext<ECommerceDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderTransaction, OrderTransaction>();
            services.AddScoped<IOrderUnitOfWork>(
                    sp => sp.GetRequiredService<ECommerceDbContext>());

            return services;
        }
    }
}
