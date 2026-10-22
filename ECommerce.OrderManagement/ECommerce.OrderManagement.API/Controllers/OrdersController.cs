using ECommerce.OrderManagement.Application.Orders;
using ECommerce.OrderManagement.Application.Requests;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.OrderManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class OrdersController : ControllerBase
    {
        private readonly CreateOrderService _createOrderService;
        private readonly GetOrderByIdService _getOrderByIdService;
        private readonly GetOrdersService _getOrdersService;
        private readonly CancelOrderService _cancelOrderService;

        public OrdersController(CreateOrderService createOrderService, GetOrderByIdService getOrderByIdService, GetOrdersService getOrdersService, CancelOrderService cancelOrderService)
        {
            _createOrderService = createOrderService;
            _getOrderByIdService = getOrderByIdService;
            _getOrdersService = getOrdersService;
            _cancelOrderService = cancelOrderService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateOrderRequest request,
            CancellationToken cancellationToken)
        {
            var order = await _createOrderService.CreateAsync(
                request,
                cancellationToken);

            var response = new
            {
                order.Id,
                order.Status,
                order.CreatedAtUtc,
                order.TotalAmount,
                Items = order.Items.Select(item => new
                {
                    item.ProductId,
                    item.ProductName,
                    item.ProductSku,
                    item.UnitPrice,
                    item.Quantity,
                    item.LineTotal
                })
            };

            return CreatedAtAction(
                nameof(GetById),
                new { id = order.Id },
                response);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
             Guid id,
             CancellationToken cancellationToken)
        {
            var order = await _getOrderByIdService.GetByIdAsync(
                id,
                cancellationToken);

            if (order is null)
                return NotFound();

            return Ok(order);
        }

        [HttpGet]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _getOrdersService.GetPagedAsync(
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(result);
        }

        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(
        Guid id,
        CancellationToken cancellationToken)
        {
            await _cancelOrderService.CancelAsync(
                id,
                cancellationToken);

            return NoContent();
        }
    }
}
