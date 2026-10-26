using ECommerce.OrderManagement.Application.Orders;
using ECommerce.OrderManagement.Application.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.OrderManagement.API.Controllers
{
    [Authorize]
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
        [EnableRateLimiting("CreateOrderPolicy")]
        public async Task<IActionResult> Create(
            [FromBody] CreateOrderRequest request,
            [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
            CancellationToken cancellationToken)
        {
            var result = await _createOrderService.CreateAsync(
                request,
                idempotencyKey,
                cancellationToken);

            return new ContentResult
            {
                StatusCode = result.StatusCode,
                Content = result.ResponseBody,
                ContentType = "application/json"
            };
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
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Not Found",
                    Instance = HttpContext.Request.Path
                });
            }

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

        [Authorize(Roles = "Admin")]
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
