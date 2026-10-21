using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application.Products.DTOs
{
    public sealed record ProductDto(
     Guid Id,
     string Name,
     string Sku,
     decimal Price,
     int Stock,
     bool IsActive,
     DateTime CreatedAtUtc);
}
