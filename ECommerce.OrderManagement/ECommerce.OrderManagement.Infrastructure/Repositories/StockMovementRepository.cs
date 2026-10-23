using ECommerce.OrderManagement.Application.Interfaces;
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
    public sealed class StockMovementRepository : IStockMovementRepository
    {
        private readonly ECommerceDbContext _dbContext;

        public StockMovementRepository(ECommerceDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddAsync(
            StockMovement stockMovement,
            CancellationToken cancellationToken)
        {
            await _dbContext.StockMovements.AddAsync(
                stockMovement,
                cancellationToken);
        }

        public async Task<IReadOnlyList<StockMovement>> GetByProductIdAsync(
         Guid productId,
         int pageNumber,
         int pageSize,
         CancellationToken cancellationToken)
        {
            return await _dbContext.StockMovements
                .AsNoTracking()
                .Where(x => x.ProductId == productId)
                .OrderByDescending(x => x.CreatedAtUtc)
                .ThenByDescending(x => x.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }
    }
    
}
