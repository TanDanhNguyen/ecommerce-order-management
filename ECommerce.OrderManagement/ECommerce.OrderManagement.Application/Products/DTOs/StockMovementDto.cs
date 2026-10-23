using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application.Products.DTOs
{
    public sealed class StockMovementDto
    {
        public Guid Id { get; init; }
        public Guid ProductId { get; init; }
        public int Quantity { get; init; }
        public int StockAfter { get; init; }
        public string Operation { get; init; } = string.Empty;
        public DateTime CreatedAtUtc { get; init; }
    }
}
