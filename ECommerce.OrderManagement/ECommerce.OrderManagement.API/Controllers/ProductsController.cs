using ECommerce.OrderManagement.API.Models.Products;
using ECommerce.OrderManagement.API.Requests;
using ECommerce.OrderManagement.Application;
using ECommerce.OrderManagement.Application.Products;
using ECommerce.OrderManagement.Application.StockMovements;
using ECommerce.OrderManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.OrderManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly CreateProductService _createProductService;
        private readonly GetProductsService _getProductsService;
        private readonly GetProductByIdService _getProductByIdService;
        private readonly UpdateProductService _updateProductService;
        private readonly ECommerceDbContext _dbContext;
        private readonly DeactivateProductService _deactivateProductService;
        private readonly AdjustProductStockService _adjustProductStockService;
        private readonly GetStockMovementsService _getStockMovementsService;

        public ProductsController(
            CreateProductService createProductService,
            GetProductsService getProductsService,
            GetProductByIdService getProductByIdService,
            UpdateProductService updateProductService,
            ECommerceDbContext dbContext,
            DeactivateProductService deactivateProductService,
            AdjustProductStockService adjustProductStockService,
            GetStockMovementsService getStockMovementsService)
        {
            _createProductService = createProductService;
            _getProductsService = getProductsService;
            _getProductByIdService = getProductByIdService;
            _updateProductService = updateProductService;
            _dbContext = dbContext;
            _deactivateProductService = deactivateProductService;
            _adjustProductStockService = adjustProductStockService;
            _getStockMovementsService = getStockMovementsService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateProductRequest request,
            CancellationToken cancellationToken)
        {
            var id = await _createProductService.CreateAsync(
                request.Name,
                request.Sku,
                request.Price,
                request.Stock,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id },
                new { id });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var product = await _getProductByIdService.GetByIdAsync(
                id,
                cancellationToken);

            if (product is null)
                return NotFound();

            return Ok(new
            {
                product.Id,
                product.Name,
                product.Sku,
                product.Price,
                product.Stock,
                product.IsActive,
                product.CreatedAtUtc
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDescending = false,
            CancellationToken cancellationToken = default)
        {
            var result = await _getProductsService.GetPagedAsync(
                pageNumber,
                pageSize,
                search,
                isActive,
                sortBy,
                sortDescending,
                cancellationToken);

            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateProductRequest request,
            CancellationToken cancellationToken)
        {
            var updated = await _updateProductService.UpdateAsync(
                id,
                request.Name,
                request.Sku,
                request.Price,
                request.Stock,
                cancellationToken);

            if (!updated)
                return NotFound();

            return NoContent();
        }

        [HttpPatch("{id:guid}/deactivate")]
        public async Task<IActionResult> Deactivate(
            Guid id,
            CancellationToken cancellationToken)
        {
            var deactivated = await _deactivateProductService.DeactivateAsync(
                id,
                cancellationToken);

            if (!deactivated)
                return NotFound();

            return NoContent();
        }

        [HttpPatch("{id:guid}/stock")]
        public async Task<IActionResult> AdjustStock(
            Guid id,
            [FromBody] AdjustProductStockRequest request,
            CancellationToken cancellationToken)
        {
            var operation = request.Operation == "increase"
                 ? StockAdjustmentOperation.Increase
                 : StockAdjustmentOperation.Decrease;

            var adjusted = await _adjustProductStockService.AdjustAsync(
                id,
                request.Quantity,
                operation,
                cancellationToken);

            if (!adjusted)
                return NotFound();

            return NoContent();
        }
        [HttpGet("{productId:guid}/stock-movements")]
        public async Task<IActionResult> GetStockMovements(
            Guid productId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _getStockMovementsService.ExecuteAsync(
                productId,
                pageNumber,
                pageSize,
                cancellationToken);

            if (result is null)
                return NotFound();

            return Ok(result);
        }
    }
}
