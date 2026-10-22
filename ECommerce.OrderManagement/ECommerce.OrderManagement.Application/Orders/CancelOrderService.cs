using ECommerce.OrderManagement.Application.Interfaces;
using ECommerce.OrderManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application.Orders
{
    public sealed class CancelOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderTransaction _orderTransaction;

        public CancelOrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IOrderTransaction orderTransaction)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _orderTransaction = orderTransaction;
        }

        public async Task CancelAsync(
            Guid orderId,
            CancellationToken cancellationToken)
        {
            if (orderId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Order ID is required.",
                    nameof(orderId));
            }

            var order = await _orderRepository.GetByIdAsync(
                orderId,
                cancellationToken);

            if (order is null)
            {
                throw new KeyNotFoundException("Order not found.");
            }

            if (order.Status != OrderStatus.Pending &&
                order.Status != OrderStatus.Confirmed)
            {
                throw new InvalidOperationException(
                    "Only pending or confirmed orders can be cancelled.");
            }

            var quantitiesByProduct = order.Items
                .GroupBy(item => item.ProductId)
                .ToDictionary(
                    group => group.Key,
                    group => group.Sum(item => item.Quantity));

            var products = await _productRepository.GetByIdsAsync(
                quantitiesByProduct.Keys.ToArray(),
                cancellationToken);

            var productsById = products.ToDictionary(product => product.Id);

            if (quantitiesByProduct.Keys.Any(id => !productsById.ContainsKey(id)))
            {
                throw new InvalidOperationException(
                    "One or more products belonging to this order no longer exist.");
            }

            await _orderTransaction.ExecuteAsync(async ct =>
            {

                foreach (var (productId, quantity) in quantitiesByProduct)
                {
                    productsById[productId].IncreaseStock(quantity);
                }

                order.Cancel();

                await Task.CompletedTask;
            }, cancellationToken);
        }
    }
}
