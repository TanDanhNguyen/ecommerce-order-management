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
    public class IdempotencyRepository : IIdempotencyRepository
    {
        private readonly ECommerceDbContext _dbContext;

        public IdempotencyRepository(ECommerceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IdempotencyRecord?> GetByUserAndKeyAsync(
            Guid userId,
            string key,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            return await _dbContext.IdempotencyRecords
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.UserId == userId
                        && x.Key == key
                        && x.ExpiresAtUtc > now,
                    cancellationToken);
        }

        public async Task AddAsync(
            IdempotencyRecord record,
            CancellationToken cancellationToken)
        {
            await _dbContext.IdempotencyRecords.AddAsync(
                record,
                cancellationToken);
        }

        public async Task SaveChangesAsync(
            CancellationToken cancellationToken)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> DeleteExpiredAsync(
            DateTime nowUtc,
            CancellationToken cancellationToken)
        {
            return await _dbContext.IdempotencyRecords
                .Where(x => x.ExpiresAtUtc <= nowUtc)
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}
