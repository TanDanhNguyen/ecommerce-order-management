using ECommerce.OrderManagement.Application.Products;
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
        private readonly ECommerceDbContext _dbContext;

        public ProductsController(
            CreateProductService createProductService,
            GetProductsService getProductsService,
            GetProductByIdService getProductByIdService,
            ECommerceDbContext dbContext)
        {
            _createProductService = createProductService;
            _getProductsService = getProductsService;
            _getProductByIdService = getProductByIdService;
            _dbContext = dbContext;
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
            CancellationToken cancellationToken = default)
        {
            var result = await _getProductsService.GetPagedAsync(
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(result);
        }

    }

    public class CreateProductRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Sku { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "9999999999999999")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }
    }
}
