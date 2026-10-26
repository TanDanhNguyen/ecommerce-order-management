using ECommerce.OrderManagement.Application.Common;
using ECommerce.OrderManagement.Application.Interfaces;
using ECommerce.OrderManagement.Application.Products.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application.StockMovements
{
    public sealed class GetStockMovementsService
    {
        private readonly IStockMovementRepository _stockMovementRepository;
        private readonly IProductRepository _productRepository;

        public GetStockMovementsService(
            IStockMovementRepository stockMovementRepository,
            IProductRepository productRepository)
        {
            _stockMovementRepository = stockMovementRepository;
            _productRepository = productRepository;
        }

        public async Task<PagedResult<StockMovementDto>?> ExecuteAsync(
            Guid productId,
            int pageNumber,
            int pageSize,
            string? operation,
            CancellationToken cancellationToken)
        {
            if (productId == Guid.Empty)
                throw new ArgumentException("ProductId is required.");

            if (pageNumber < 1)
                throw new ArgumentOutOfRangeException(
                    nameof(pageNumber),
                    "Page number must be greater than zero.");

            if (pageSize < 1 || pageSize > 100)
                throw new ArgumentOutOfRangeException(
                    nameof(pageSize),
                    "Page size must be between 1 and 100.");

            if (!string.IsNullOrWhiteSpace(operation))
            {
                var allowedOperations = new[]
                {
                    "Order",
                    "Increase",
                    "Decrease"
                };

                if (!allowedOperations.Contains(
                        operation,
                        StringComparer.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(
                        "Operation must be Order, Increase, or Decrease.",
                        nameof(operation));
                }

                operation = allowedOperations.First(
                    x => string.Equals(
                        x,
                        operation,
                        StringComparison.OrdinalIgnoreCase));
            }

            var product = await _productRepository.GetByIdAsync(
                productId,
                cancellationToken);

            if (product is null)
                return null;

            var movements = await _stockMovementRepository.GetByProductIdAsync(
                productId,
                pageNumber,
                pageSize,
                operation,
                cancellationToken);

            var totalCount = await _stockMovementRepository.CountByProductIdAsync(
                productId,
                operation,
                cancellationToken);

            return new PagedResult<StockMovementDto>
            {
                Items = movements
                    .Select(x => new StockMovementDto
                    {
                        Id = x.Id,
                        ProductId = x.ProductId,
                        Quantity = x.Quantity,
                        StockAfter = x.StockAfter,
                        Operation = x.Operation,
                        CreatedAtUtc = x.CreatedAtUtc
                    })
                    .ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}
