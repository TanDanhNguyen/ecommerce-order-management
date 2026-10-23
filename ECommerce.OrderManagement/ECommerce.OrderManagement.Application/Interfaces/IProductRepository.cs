using ECommerce.OrderManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application.Interfaces
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken);

        Task AddAsync(
            Product product,
            CancellationToken cancellationToken);
        Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? search,
            bool? isActive,
            string? sortBy,
            bool sortDescending,
            CancellationToken cancellationToken);

        Task<bool> ExistsBySkuAsync(
            string sku,
            Guid? excludeProductId,
            CancellationToken cancellationToken);

        Task UpdateAsync(CancellationToken cancellationToken);

        Task<IReadOnlyList<Product>> GetByIdsAsync(
            IReadOnlyCollection<Guid> ids,
            CancellationToken cancellationToken);
    }
}
