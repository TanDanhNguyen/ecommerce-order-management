using ECommerce.OrderManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application.Interfaces
{
    public interface IStockMovementRepository
    {
        Task AddAsync(
            StockMovement stockMovement,
            CancellationToken cancellationToken);
        Task<IReadOnlyList<StockMovement>> GetByProductIdAsync(
            Guid productId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken);
    }
}
