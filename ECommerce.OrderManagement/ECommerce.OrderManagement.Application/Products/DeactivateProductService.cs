using ECommerce.OrderManagement.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application.Products
{

    public class DeactivateProductService
    {
        private readonly IProductRepository _productRepository;

        public DeactivateProductService(
            IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<bool> DeactivateAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(
                id,
                cancellationToken);

            if (product is null)
                return false;

            if (!product.IsActive)
                return false;

            product.Deactivate();

            await _productRepository.UpdateAsync(
                cancellationToken);

            return true;
        }
    }

}
