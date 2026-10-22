using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Domain.Entities
{
    public class OrderItem
    {
        public Guid Id { get; private set; }

        public Guid ProductId { get; private set; }

        public string ProductName { get; private set; } = string.Empty;

        public string ProductSku { get; private set; } = string.Empty;

        public decimal UnitPrice { get; private set; }

        public int Quantity { get; private set; }

        public decimal LineTotal => UnitPrice * Quantity;

        private OrderItem()
        {
        }

        public OrderItem(
            Guid productId,
            string productName,
            string productSku,
            decimal unitPrice,
            int quantity)
        {
            if (productId == Guid.Empty)
                throw new ArgumentException(
                    "Product ID is required.", nameof(productId));

            if (string.IsNullOrWhiteSpace(productName))
                throw new ArgumentException(
                    "Product name is required.", nameof(productName));

            if (string.IsNullOrWhiteSpace(productSku))
                throw new ArgumentException(
                    "Product SKU is required.", nameof(productSku));

            if (unitPrice <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(unitPrice), "Unit price must be greater than zero.");

            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(quantity), "Quantity must be greater than zero.");

            Id = Guid.NewGuid();
            ProductId = productId;
            ProductName = productName.Trim();
            ProductSku = productSku.Trim();
            UnitPrice = unitPrice;
            Quantity = quantity;
        }
    }
}
