using ECommerce.OrderManagement.Application.Exceptions;
using ECommerce.OrderManagement.Application.Interfaces;

namespace ECommerce.OrderManagement.Application.Products
{
    public class UpdateProductService
    {
        private readonly IProductRepository _productRepository;

        public UpdateProductService(
            IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
        public async Task<bool> UpdateAsync(
            Guid id,
            string name,
            string sku,
            decimal price,
            int stock,
            CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(
                id,
                cancellationToken);

            if (product is null)
                return false;

            if (!product.IsActive)
                throw new InvalidOperationException(
                    $"Product '{product.Name}' is inactive and cannot be updated.");

            var normalizedSku = sku?.Trim() ?? string.Empty;

            var skuExists = await _productRepository.ExistsBySkuAsync(
                normalizedSku,
                id,
                cancellationToken);

            if (skuExists)
            {
                throw new DuplicateProductSkuException(normalizedSku);
            }

            product.UpdateDetails(name, normalizedSku);
            product.UpdatePrice(price);
            product.SetStock(stock);

            await _productRepository.UpdateAsync(
                cancellationToken);

            return true;
        }
    }
}
