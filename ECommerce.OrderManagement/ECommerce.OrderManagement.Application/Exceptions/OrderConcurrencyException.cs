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
            : base(
                "The operation could not be completed because the data was modified by another request. Please try again.")
        {
        }
    }
}
    
