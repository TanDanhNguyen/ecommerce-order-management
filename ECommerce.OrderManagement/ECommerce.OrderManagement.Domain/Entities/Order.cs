using ECommerce.OrderManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Domain.Entities
{
    public class Order
    {
        private readonly List<OrderItem> _items = new();

        public Guid Id { get; private set; }

        public DateTime CreatedAtUtc { get; private set; }

        public OrderStatus Status { get; private set; }

        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

        public decimal TotalAmount => _items.Sum(item => item.LineTotal);

        private Order()
        {
        }

        public static Order Create()
        {
            return new Order
            {
                Id = Guid.NewGuid(),
                CreatedAtUtc = DateTime.UtcNow,
                Status = OrderStatus.Pending
            };
        }

        public void AddItem(OrderItem item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            _items.Add(item);
        }

        public void Confirm()
        {
            if (Status != OrderStatus.Pending)
                throw new InvalidOperationException(
                    "Only pending orders can be confirmed.");

            if (_items.Count == 0)
                throw new InvalidOperationException(
                    "Cannot confirm an empty order.");

            Status = OrderStatus.Confirmed;
        }

        public void Cancel()
        {
            if (Status != OrderStatus.Pending &&
                Status != OrderStatus.Confirmed)
            {
                throw new InvalidOperationException(
                    "Only pending or confirmed orders can be cancelled.");
            }

            Status = OrderStatus.Cancelled;
        }

        public void Complete()
        {
            if (Status != OrderStatus.Confirmed)
                throw new InvalidOperationException(
                    "Only confirmed orders can be completed.");

            Status = OrderStatus.Completed;
        }
    }
}
