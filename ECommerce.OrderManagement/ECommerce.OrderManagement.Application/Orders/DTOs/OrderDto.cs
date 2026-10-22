using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application.Orders.DTOs
{
    public sealed record OrderDto(
    Guid Id,
    string Status,
    DateTime CreatedAtUtc,
    decimal TotalAmount,
    IReadOnlyList<OrderItemDto> Items);

    public sealed record OrderItemDto(
        Guid ProductId,
        string ProductName,
        string ProductSku,
        decimal UnitPrice,
        int Quantity,
        decimal LineTotal);
}
