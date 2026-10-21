using ECommerce.OrderManagement.Application.Interfaces;
using ECommerce.OrderManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application.Products
{
    public class CreateProductService
    {
        private readonly IProductRepository _productRepository;

        public CreateProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<Guid> CreateAsync(
            string name,
            string sku,
            decimal price,
            int stock,
            CancellationToken cancellationToken)
        {
            var product = new Product(name, sku, price, stock);

            await _productRepository.AddAsync(
                product,
                cancellationToken);

            return product.Id;
        }
    }
}
