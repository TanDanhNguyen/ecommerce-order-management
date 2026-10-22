using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application.Products.DTOs
{
    public sealed record ProductPagedResult<T>(
       IReadOnlyList<T> Items,
       int PageNumber,
       int PageSize,
       int TotalCount);
}
