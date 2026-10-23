using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Domain.Entities
{
    public sealed class StockMovement
    {
        public Guid Id { get; private set; }

        public Guid ProductId { get; private set; }

        public int Quantity { get; private set; }

        public int StockAfter { get; private set; }

        public string Operation { get; private set; } = string.Empty;

        public DateTime CreatedAtUtc { get; private set; }

        private StockMovement()
        {
        }

        public StockMovement(
            Guid productId,
            int quantity,
            int stockAfter,
            string operation)
        {
            if (productId == Guid.Empty)
                throw new ArgumentException(
                    "ProductId is required.",
                    nameof(productId));

            if (quantity == 0)
                throw new ArgumentOutOfRangeException(
                    nameof(quantity),
                    "Quantity cannot be zero.");

            if (stockAfter < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(stockAfter),
                    "StockAfter cannot be negative.");

            if (string.IsNullOrWhiteSpace(operation))
                throw new ArgumentException(
                    "Operation is required.",
                    nameof(operation));

            Id = Guid.NewGuid();
            ProductId = productId;
            Quantity = quantity;
            StockAfter = stockAfter;
            Operation = operation.Trim();
            CreatedAtUtc = DateTime.UtcNow;
        }
    }
}
