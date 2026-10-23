using ECommerce.OrderManagement.Application.Exceptions;
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
    public class ProductRepository : IProductRepository
    {
        private readonly ECommerceDbContext _dbContext;

        public ProductRepository(ECommerceDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Product?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Products
                .FirstOrDefaultAsync(
                    p => p.Id == id,
                    cancellationToken);
        }

        public async Task AddAsync(
            Product product,
            CancellationToken cancellationToken)
        {
            await _dbContext.Products.AddAsync(
                product,
                cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? search,
            bool? isActive,
            string? sortBy,
            bool sortDescending,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Products
                .AsNoTracking()
                .AsQueryable();

            // 1. Search by product name or SKU
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();

                query = query.Where(p =>
                    p.Name.Contains(keyword) ||
                    p.Sku.Contains(keyword));
            }

            // 2. Filter by active status
            if (isActive.HasValue)
            {
                query = query.Where(p => p.IsActive == isActive.Value);
            }

            // 3. Count filtered products before pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // 4. Apply sorting using allowed fields only
            var normalizedSortBy = sortBy?.Trim().ToLowerInvariant();

            query = normalizedSortBy switch
            {
                "name" => sortDescending
                    ? query.OrderByDescending(p => p.Name).ThenBy(p => p.Id)
                    : query.OrderBy(p => p.Name).ThenBy(p => p.Id),

                "price" => sortDescending
                    ? query.OrderByDescending(p => p.Price).ThenBy(p => p.Id)
                    : query.OrderBy(p => p.Price).ThenBy(p => p.Id),

                "createdatutc" => sortDescending
                    ? query.OrderByDescending(p => p.CreatedAtUtc).ThenBy(p => p.Id)
                    : query.OrderBy(p => p.CreatedAtUtc).ThenBy(p => p.Id),

                _ => query.OrderBy(p => p.CreatedAtUtc).ThenBy(p => p.Id)
            };

            // 5. Apply pagination after sorting
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<bool> ExistsBySkuAsync(
            string sku,
            Guid? excludeProductId,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Products
                .Where(p => p.Sku == sku);

            if (excludeProductId.HasValue)
            {
                query = query.Where(
                    p => p.Id != excludeProductId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }
        public async Task UpdateAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ProductConcurrencyException();
            }
        }

        public async Task<IReadOnlyList<Product>> GetByIdsAsync(
            IReadOnlyCollection<Guid> ids,
            CancellationToken cancellationToken)
        {
            if (ids.Count == 0)
            {
                return Array.Empty<Product>();
            }

            return await _dbContext.Products
                .Where(p => ids.Contains(p.Id))
                .ToListAsync(cancellationToken);
        }
    }
}
