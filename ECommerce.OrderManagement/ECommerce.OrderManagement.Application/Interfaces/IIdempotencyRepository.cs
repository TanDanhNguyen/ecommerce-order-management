using ECommerce.OrderManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application.Interfaces
{
    public interface IIdempotencyRepository
    {
        Task<IdempotencyRecord?> GetByUserAndKeyAsync(
            Guid userId,
            string key,
            CancellationToken cancellationToken);

        Task AddAsync(
            IdempotencyRecord record,
            CancellationToken cancellationToken);

        Task SaveChangesAsync(
            CancellationToken cancellationToken);

        Task<int> DeleteExpiredAsync(
            DateTime nowUtc,
            CancellationToken cancellationToken);
    }
}
