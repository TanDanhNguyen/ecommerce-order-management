using ECommerce.OrderManagement.Application.Interfaces;
using ECommerce.OrderManagement.Application.Orders.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application.Orders
{
    public sealed class GetOrderByIdService
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrderByIdService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<OrderDto?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            if (id == Guid.Empty)
                throw new ArgumentException(
                    "Order ID is required.",
                    nameof(id));

            var order = await _orderRepository.GetByIdAsync(
                id,
                cancellationToken);

            if (order is null)
                return null;

            var items = order.Items
                .Select(item => new OrderItemDto(
                    item.ProductId,
                    item.ProductName,
                    item.ProductSku,
                    item.UnitPrice,
                    item.Quantity,
                    item.LineTotal))
                .ToList();

            return new OrderDto(
                order.Id,
                order.Status.ToString(),
                order.CreatedAtUtc,
                order.TotalAmount,
                items);
        }
    }
}
