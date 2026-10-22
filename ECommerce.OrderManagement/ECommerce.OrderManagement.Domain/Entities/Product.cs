using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Domain.Entities
{
    public class Product
    {
        public Guid Id { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public string Sku { get; private set; } = string.Empty;

        public decimal Price { get; private set; }

        public int Stock { get; private set; }

        public bool IsActive { get; private set; }

        public DateTime CreatedAtUtc { get; private set; }

        public Guid Version { get; private set; } = Guid.NewGuid();

        private Product()
        {
        }

        public Product(
            string name,
            string sku,
            decimal price,
            int stock)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(
                    "Product name is required.", nameof(name));

            if (string.IsNullOrWhiteSpace(sku))
                throw new ArgumentException(
                    "SKU is required.", nameof(sku));

            if (price <= 0)
                throw new ArgumentOutOfRangeException(nameof(price));

            if (stock < 0)
                throw new ArgumentOutOfRangeException(nameof(stock));

            Id = Guid.NewGuid();
            Name = name.Trim();
            Sku = sku.Trim();
            Price = price;
            Stock = stock;
            IsActive = true;
            CreatedAtUtc = DateTime.UtcNow;
        }

        public void UpdatePrice(decimal price)
        {
            if (price <= 0)
                throw new ArgumentOutOfRangeException(nameof(price));

            Price = price;
            Version = Guid.NewGuid();
        }

        public void DecreaseStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity));

            if (Stock < quantity)
                throw new InvalidOperationException(
                    "Insufficient stock.");

            Stock -= quantity;
            Version = Guid.NewGuid();
        }

        public void IncreaseStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity));

            Stock = checked(Stock + quantity);
            Version = Guid.NewGuid();
        }

        public void Deactivate()
        {
            IsActive = false;
            Version = Guid.NewGuid();
        }
        public void UpdateDetails(string name, string sku)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(
                    "Product name is required.",
                    nameof(name));

            if (name.Trim().Length > 200)
                throw new ArgumentException(
                    "Product name cannot exceed 200 characters.",
                    nameof(name));

            if (string.IsNullOrWhiteSpace(sku))
                throw new ArgumentException(
                    "Product SKU is required.",
                    nameof(sku));

            if (sku.Trim().Length > 50)
                throw new ArgumentException(
                    "Product SKU cannot exceed 50 characters.",
                    nameof(sku));

            Name = name.Trim();
            Sku = sku.Trim();
            Version = Guid.NewGuid();
        }
        public void SetStock(int stock)
        {
            if (stock < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(stock),
                    "Stock cannot be negative.");

            Stock = stock;
            Version = Guid.NewGuid();
        }
    }
}
