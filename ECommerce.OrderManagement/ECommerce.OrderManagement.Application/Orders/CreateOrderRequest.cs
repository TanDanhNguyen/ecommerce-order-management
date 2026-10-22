namespace ECommerce.OrderManagement.Application.Requests
{
    public sealed record CreateOrderRequest(
     IReadOnlyList<CreateOrderItemRequest> Items);

    public sealed record CreateOrderItemRequest(
        Guid ProductId,
        int Quantity);
}
