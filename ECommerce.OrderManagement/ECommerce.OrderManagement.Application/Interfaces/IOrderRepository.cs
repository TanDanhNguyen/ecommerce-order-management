using ECommerce.OrderManagement.Application.Orders.DTOs;
using ECommerce.OrderManagement.Domain.Entities;

namespace ECommerce.OrderManagement.Application.Interfaces
{
    public interface IOrderRepository
    {
        Task AddAsync(
            Order order,
            CancellationToken cancellationToken);

        Task<Order?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken);

        Task<(IReadOnlyList<OrderListItemDto> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken);

    }
}
