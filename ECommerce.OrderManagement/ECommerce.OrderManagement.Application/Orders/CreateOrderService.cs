using ECommerce.OrderManagement.Application.Authentication.Interfaces;
using ECommerce.OrderManagement.Application.Exceptions;
using ECommerce.OrderManagement.Application.Idempotency;
using ECommerce.OrderManagement.Application.Interfaces;
using ECommerce.OrderManagement.Application.Orders.DTOs;
using ECommerce.OrderManagement.Application.Requests;
using ECommerce.OrderManagement.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ECommerce.OrderManagement.Application.Orders
{
    public sealed class CreateOrderService
    {
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderTransaction _orderTransaction;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStockMovementRepository _stockMovementRepository;
        private readonly IIdempotencyRepository _idempotencyRepository;

        public CreateOrderService(
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            IOrderTransaction orderTransaction,
            ICurrentUserService currentUserService,
            IStockMovementRepository stockMovementRepository,
            IIdempotencyRepository idempotencyRepository)
        {
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _orderTransaction = orderTransaction;
            _currentUserService = currentUserService;
            _idempotencyRepository = idempotencyRepository;
            _stockMovementRepository = stockMovementRepository;
        }

        public async Task<IdempotencyResult> CreateAsync(
            CreateOrderRequest request,
            string idempotencyKey,
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

            var userId = _currentUserService.UserId;

            if (userId is null || userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "An authenticated user is required to create an order.");
            }

            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                throw new ArgumentException(
                    "Idempotency-Key header is required.",
                    nameof(idempotencyKey));
            }

            if (idempotencyKey.Length > 100)
            {
                throw new ArgumentException(
                    "Idempotency-Key must not exceed 100 characters.",
                    nameof(idempotencyKey));
            }

            var requestHash = RequestHashHelper.Compute(request);

            var existingRecord =
                await _idempotencyRepository.GetByUserAndKeyAsync(
                    userId.Value,
                    idempotencyKey,
                    cancellationToken);

            if (existingRecord is not null)
            {
                if (existingRecord.RequestHash != requestHash)
                {
                    throw new InvalidOperationException(
                        "Idempotency-Key was already used with a different request.");
                }

                return new IdempotencyResult(
                    existingRecord.StatusCode,
                    existingRecord.ResponseBody,
                    true);
            }

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

            var order = Order.Create(userId.Value);
            var responseBody = string.Empty;
            try
            {

            
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
                    var response = CreateOrderResponseMapper.Map(order);

                    responseBody = JsonSerializer.Serialize(
                        response,
                        new JsonSerializerOptions(JsonSerializerDefaults.Web));

                    var now = DateTime.UtcNow;

                    var record = new IdempotencyRecord(
                        userId.Value,
                        idempotencyKey,
                        requestHash,
                        201,
                        responseBody,
                        now,
                        now.AddHours(24));

                    await _idempotencyRepository.AddAsync(record, ct);
                },
                cancellationToken);
            }
            catch (DbUpdateException ex)
                when (IsIdempotencyDuplicateKey(ex))
            {
                var savedRecord =
                    await _idempotencyRepository.GetByUserAndKeyAsync(
                        userId.Value,
                        idempotencyKey,
                        cancellationToken);

                if (savedRecord is null)
                {
                    throw;
                }

                if (savedRecord.RequestHash != requestHash)
                {
                    throw new InvalidOperationException(
                        "Idempotency-Key was already used with a different request.");
                }

                return new IdempotencyResult(
                    savedRecord.StatusCode,
                    savedRecord.ResponseBody,
                    true);
            }

            return new IdempotencyResult(
                201,
                responseBody,
                false);
        }

        private static bool IsIdempotencyDuplicateKey(Exception exception)
        {
            while (exception is not null)
            {
                if (exception is SqlException sqlException)
                {
                    var isDuplicateKey = sqlException.Number is 2601 or 2627;

                    var isIdempotencyIndex =
                        sqlException.Message.Contains(
                            "IdempotencyRecords",
                            StringComparison.OrdinalIgnoreCase);

                    return isDuplicateKey && isIdempotencyIndex;
                }

                exception = exception.InnerException!;
            }

            return false;
        }
    }
}
