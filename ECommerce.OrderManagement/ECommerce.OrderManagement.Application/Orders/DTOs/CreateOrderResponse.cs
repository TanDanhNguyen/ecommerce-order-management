using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application.Orders.DTOs
{
    public sealed record CreateOrderResponse(
        Guid Id,
        string Status,
        DateTime CreatedAtUtc,
        decimal TotalAmount,
        IReadOnlyList<CreateOrderItemResponse> Items);

    public sealed record CreateOrderItemResponse(
        Guid ProductId,
        string ProductName,
        string ProductSku,
        decimal UnitPrice,
        int Quantity,
        decimal LineTotal);
}
