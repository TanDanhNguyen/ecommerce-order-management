using ECommerce.OrderManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application.Interfaces
{
    public interface IOrderRepository
    {
        Task AddAsync(
            Order order,
            CancellationToken cancellationToken);
    }
}
