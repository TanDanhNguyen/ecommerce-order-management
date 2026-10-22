using ECommerce.OrderManagement.Application.Interfaces;
using ECommerce.OrderManagement.Application.Products.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application.Products
{
    public class GetProductsService
    {

        private readonly IProductRepository _productRepository;

        public GetProductsService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<ProductPagedResult<ProductDto>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
        {
            if (pageNumber < 1)
                throw new ArgumentOutOfRangeException(
                    nameof(pageNumber),
                    "Page number must be greater than zero.");

            if (pageSize < 1 || pageSize > 100)
                throw new ArgumentOutOfRangeException(
                    nameof(pageSize),
                    "Page size must be between 1 and 100.");

            var (products, totalCount) =
                await _productRepository.GetPagedAsync(
                    pageNumber,
                    pageSize,
                    cancellationToken);

            var items = products
                .Select(p => new ProductDto(
                    p.Id,
                    p.Name,
                    p.Sku,
                    p.Price,
                    p.Stock,
                    p.IsActive,
                    p.CreatedAtUtc))
                .ToList();

            return new ProductPagedResult<ProductDto>(
                items,
                pageNumber,
                pageSize,
                totalCount);
        }
    }
}
