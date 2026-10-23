using ECommerce.OrderManagement.Application.Interfaces;
using ECommerce.OrderManagement.Application.Orders.DTOs;
using ECommerce.OrderManagement.Domain.Entities;
using ECommerce.OrderManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
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

        public async Task<Order?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(
                    o => o.Id == id,
                    cancellationToken);
        }

        public async Task<(
            IReadOnlyList<OrderListItemDto> Items,
            int TotalCount)> GetPagedAsync(
                Guid userId,
                int pageNumber,
                int pageSize,
                CancellationToken cancellationToken)
        {
            var query = _dbContext.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAtUtc)
                .ThenByDescending(o => o.Id);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(order => new OrderListItemDto(
                    order.Id,
                    order.Status.ToString(),
                    order.CreatedAtUtc,
                    order.Items.Sum(item => item.UnitPrice * item.Quantity),
                    order.Items.Sum(item => item.Quantity)))
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
}
