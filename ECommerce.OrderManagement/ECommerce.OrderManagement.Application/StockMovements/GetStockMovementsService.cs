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

        public async Task<IReadOnlyList<StockMovementDto>?> ExecuteAsync(
    Guid productId,
    int pageNumber,
    int pageSize,
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

            var product = await _productRepository.GetByIdAsync(
                productId,
                cancellationToken);

            if (product is null)
                return null;

            var movements = await _stockMovementRepository.GetByProductIdAsync(
                productId,
                pageNumber,
                pageSize,
                cancellationToken);

            return movements
                .Select(x => new StockMovementDto
                {
                    Id = x.Id,
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    StockAfter = x.StockAfter,
                    Operation = x.Operation,
                    CreatedAtUtc = x.CreatedAtUtc
                })
                .ToList();
        }
    }
}
