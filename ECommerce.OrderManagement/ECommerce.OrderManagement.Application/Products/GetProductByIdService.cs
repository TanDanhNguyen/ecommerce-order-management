using ECommerce.OrderManagement.Application.Interfaces;
using ECommerce.OrderManagement.Application.Products.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application.Products
{
    public class GetProductByIdService
    {
        private readonly IProductRepository _productRepository;

        public GetProductByIdService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<ProductDto?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(
                id,
                cancellationToken);

            if (product is null)
                return null;

            return new ProductDto(
                product.Id,
                product.Name,
                product.Sku,
                product.Price,
                product.Stock,
                product.IsActive,
                product.CreatedAtUtc);
        }
    }
}
