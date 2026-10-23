using System.ComponentModel.DataAnnotations;

namespace ECommerce.OrderManagement.API.Models.Products
{
    public sealed class AdjustProductStockRequest
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; init; }

        [Required]
        [RegularExpression(
            "^(increase|decrease)$",
            ErrorMessage = "Operation must be 'increase' or 'decrease'.")]
        public string Operation { get; init; } = string.Empty;


    }
}
