using ECommerce.OrderManagement.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Infrastructure.BackgroundServices
{
    public sealed class IdempotencyCleanup(
        IIdempotencyRepository repository)
        : IIdempotencyCleanup
    {
        public Task<int> CleanupExpiredAsync(
            CancellationToken cancellationToken)
        {
            return repository.DeleteExpiredAsync(
                DateTime.UtcNow,
                cancellationToken);
        }
    }
}
