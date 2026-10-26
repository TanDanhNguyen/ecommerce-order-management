using ECommerce.OrderManagement.Application.Authentication;
using ECommerce.OrderManagement.Application.Authentication.Interfaces;
using ECommerce.OrderManagement.Application.Interfaces;
using ECommerce.OrderManagement.Infrastructure.Authentication;
using ECommerce.OrderManagement.Infrastructure.BackgroundServices;
using ECommerce.OrderManagement.Infrastructure.Identity;
using ECommerce.OrderManagement.Infrastructure.Persistence;
using ECommerce.OrderManagement.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
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

            services.AddDbContext<ApplicationIdentityDbContext>(options =>
                 options.UseSqlServer(connectionString));

            services
                .AddIdentityCore<ApplicationUser>(options =>
                {
                    options.User.RequireUniqueEmail = true;

                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = false;
                })
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<ApplicationIdentityDbContext>()
                .AddSignInManager();
            services.AddSingleton<TimeProvider>(TimeProvider.System);

            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderTransaction, OrderTransaction>();
            services.AddScoped<IOrderUnitOfWork>(
                    sp => sp.GetRequiredService<ECommerceDbContext>());

            services.AddScoped<IUserIdentityService, IdentityUserService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IStockMovementRepository, StockMovementRepository>();
            services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();
            services.AddHostedService<IdempotencyCleanupService>();
            services.AddScoped<IIdempotencyCleanup, IdempotencyCleanup>();
            return services;
        }
    }
}
