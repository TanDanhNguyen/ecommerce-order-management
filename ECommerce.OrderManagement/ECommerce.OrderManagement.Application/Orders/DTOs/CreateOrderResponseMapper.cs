using ECommerce.OrderManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application.Orders.DTOs
{
    public static class CreateOrderResponseMapper
    {
        public static CreateOrderResponse Map(Order order)
        {
            return new CreateOrderResponse(
                order.Id,
                order.Status.ToString(),
                order.CreatedAtUtc,
                order.TotalAmount,
                order.Items.Select(item =>
                    new CreateOrderItemResponse(
                        item.ProductId,
                        item.ProductName,
                        item.ProductSku,
                        item.UnitPrice,
                        item.Quantity,
                        item.LineTotal))
                    .ToList());
        }
    }
}
