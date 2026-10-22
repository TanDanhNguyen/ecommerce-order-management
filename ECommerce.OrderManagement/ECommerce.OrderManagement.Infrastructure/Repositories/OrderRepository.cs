using ECommerce.OrderManagement.Application.Interfaces;
using ECommerce.OrderManagement.Domain.Entities;
using ECommerce.OrderManagement.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ECommerceDbContext _dbContext;

        public OrderRepository(ECommerceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(
            Order order,
            CancellationToken cancellationToken)
        {
            await _dbContext.Orders.AddAsync(
                order,
                cancellationToken);
        }
    }
}
