using ECommerce.OrderManagement.Application.Authentication;
using ECommerce.OrderManagement.Application.Orders;
using ECommerce.OrderManagement.Application.Products;
using ECommerce.OrderManagement.Infrastructure.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.OrderManagement.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services)
        {
            services.AddScoped<CreateProductService>();
            services.AddScoped<GetProductsService>();
            services.AddScoped<GetProductByIdService>();
            services.AddScoped<UpdateProductService>();
            services.AddScoped<CreateOrderService>();
            services.AddScoped<GetOrderByIdService>();
            services.AddScoped<GetOrdersService>();
            services.AddScoped<CancelOrderService>();
            services.AddScoped<RegisterService>();
            services.AddScoped<LoginService>();
            return services;
        }
    }
}
