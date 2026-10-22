using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application.Exceptions
{
    public sealed class OrderConcurrencyException : Exception
    {
        public OrderConcurrencyException()
            : base("The order could not be created because product stock was updated by another request. Please try again.")
        {
        }
    }
}
