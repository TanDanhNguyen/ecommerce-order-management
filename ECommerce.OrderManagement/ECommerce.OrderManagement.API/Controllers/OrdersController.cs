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

        public OrdersController(CreateOrderService createOrderService)
        {
            _createOrderService = createOrderService;
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
        public IActionResult GetById(Guid id)
        {
            // Sẽ triển khai truy vấn đơn hàng ở bước sau.
            return NotFound();
        }
    }
}
