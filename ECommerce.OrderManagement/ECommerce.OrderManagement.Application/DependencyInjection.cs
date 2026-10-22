using ECommerce.OrderManagement.Application.Orders;
using ECommerce.OrderManagement.Application.Products;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            return services;
        }
    }
}
