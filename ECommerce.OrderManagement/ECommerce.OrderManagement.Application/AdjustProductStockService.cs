using ECommerce.OrderManagement.Application.Interfaces;
using ECommerce.OrderManagement.Application.Products;
using ECommerce.OrderManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application
{
    public sealed class AdjustProductStockService
    {
        private readonly IProductRepository _productRepository;
        private readonly IStockMovementRepository _stockMovementRepository;

        public AdjustProductStockService(
            IProductRepository productRepository, IStockMovementRepository stockMovementRepository)
        {
            _productRepository = productRepository;
            _stockMovementRepository = stockMovementRepository;
        }

        public async Task<bool> AdjustAsync(
            Guid id,
            int quantity,
            StockAdjustmentOperation operation,
            CancellationToken cancellationToken)
        {
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(quantity),
                    "Quantity must be greater than zero.");

            var product = await _productRepository.GetByIdAsync(
                id,
                cancellationToken);

            if (product is null)
                return false;

            if (!product.IsActive)
                throw new InvalidOperationException(
                    $"Product '{product.Name}' is inactive and cannot be adjusted.");

            switch (operation)
            {
                case StockAdjustmentOperation.Increase:
                    product.IncreaseStock(quantity);
                    break;

                case StockAdjustmentOperation.Decrease:
                    product.DecreaseStock(quantity);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(operation),
                        "Unsupported stock adjustment operation.");
            }

            var movementQuantity = operation switch
            {
                StockAdjustmentOperation.Increase => quantity,
                StockAdjustmentOperation.Decrease => -quantity,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(operation),
                    "Unsupported stock adjustment operation.")
            };

            var stockMovement = new StockMovement(
                product.Id,
                movementQuantity,
                product.Stock,
                operation.ToString());

            await _stockMovementRepository.AddAsync(
                stockMovement,
                cancellationToken);

            await _productRepository.UpdateAsync(cancellationToken);

            return true;
        }
    }
}
