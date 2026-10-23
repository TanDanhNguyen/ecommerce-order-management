using ECommerce.OrderManagement.Application.Authentication.Interfaces;
using ECommerce.OrderManagement.Application.Exceptions;
using ECommerce.OrderManagement.Application.Interfaces;
using ECommerce.OrderManagement.Application.Requests;
using ECommerce.OrderManagement.Domain.Entities;

namespace ECommerce.OrderManagement.Application.Orders
{
    public sealed class CreateOrderService
    {
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderTransaction _orderTransaction;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStockMovementRepository _stockMovementRepository;

        public CreateOrderService(
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            IOrderTransaction orderTransaction,
            ICurrentUserService currentUserService,
            IStockMovementRepository stockMovementRepository)
        {
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _orderTransaction = orderTransaction;
            _currentUserService = currentUserService;
            _stockMovementRepository = stockMovementRepository;
        }

        public async Task<Order> CreateAsync(
            CreateOrderRequest request,
            CancellationToken cancellationToken)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            if (request.Items is null || request.Items.Count == 0)
                throw new ArgumentException(
                    "Order must contain at least one item.");

            if (request.Items.Any(item => item.Quantity <= 0))
                throw new ArgumentException(
                    "Product quantity must be greater than zero.");

            var requestedQuantities = request.Items
                .GroupBy(item => item.ProductId)
                .ToDictionary(
                    group => group.Key,
                    group => group.Sum(item => item.Quantity));

            var productIds = request.Items
                .Select(item => item.ProductId)
                .Distinct()
                .ToArray();

            var products = await _productRepository.GetByIdsAsync(
                productIds,
                cancellationToken);

            if (products.Count != productIds.Length)
                throw new InvalidOperationException(
                    "One or more products do not exist.");

            var productDictionary = products.ToDictionary(p => p.Id);

            foreach (var (productId, quantity) in requestedQuantities)
            {
                var product = products.First(p => p.Id == productId);

                if (!product.IsActive)
                    throw new InvalidOperationException(
                        $"Product '{product.Name}' is inactive.");

                if (product.Stock < quantity)
                    throw new InsufficientStockException(
                        $"Insufficient stock for product '{product.Name}'.");
            }

            var userId = _currentUserService.UserId;

            if (userId is null || userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "An authenticated user is required to create an order.");
            }

            var order = Order.Create(userId.Value);

            await _orderTransaction.ExecuteAsync(
                async ct =>
                {
                    foreach (var (productId, quantity) in requestedQuantities)
                    {
                        var product = productDictionary[productId];

                        var orderItem = new OrderItem(
                            product.Id,
                            product.Name,
                            product.Sku,
                            product.Price,
                            quantity);

                        order.AddItem(orderItem);
                        product.DecreaseStock(quantity);

                        var stockMovement = new StockMovement(
                            product.Id,
                            -quantity,
                            product.Stock,
                            "Order");

                        await _stockMovementRepository.AddAsync(
                            stockMovement,
                            ct);
                    }

                    await _orderRepository.AddAsync(order, ct);
                },
                cancellationToken);

            return order;
        }
    }
}
