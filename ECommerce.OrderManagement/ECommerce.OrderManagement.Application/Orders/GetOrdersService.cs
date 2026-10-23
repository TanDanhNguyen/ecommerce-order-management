using ECommerce.OrderManagement.Application.Authentication.Interfaces;
using ECommerce.OrderManagement.Application.Interfaces;
using ECommerce.OrderManagement.Application.Orders.DTOs;

namespace ECommerce.OrderManagement.Application.Orders
{
    public sealed class GetOrdersService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetOrdersService(IOrderRepository orderRepository, ICurrentUserService currentUserService)
        {
            _orderRepository = orderRepository;
            _currentUserService = currentUserService;
        }

        public async Task<PagedResult<OrderListItemDto>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken)
        {
            if (pageNumber < 1)
                throw new ArgumentException(
                    "Page number must be greater than zero.",
                    nameof(pageNumber));

            if (pageSize < 1 || pageSize > 100)
                throw new ArgumentException(
                    "Page size must be between 1 and 100.",
                    nameof(pageSize));

            var currentUserId = _currentUserService.UserId;

            if (currentUserId is null || currentUserId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "An authenticated user is required to access orders.");
            }

            var (items, totalCount) =
                await _orderRepository.GetPagedAsync(
                    currentUserId.Value,
                    pageNumber,
                    pageSize,
                    cancellationToken);

            return new PagedResult<OrderListItemDto>(
                items,
                pageNumber,
                pageSize,
                totalCount);
        }
    }
}
