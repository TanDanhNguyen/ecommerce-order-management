using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application.Orders.DTOs
{
    public sealed record OrderListItemDto(
        Guid Id,
        string Status,
        DateTime CreatedAtUtc,
        decimal TotalAmount,
        int ItemCount);
}
