using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application.Exceptions
{
    public sealed class ProductConcurrencyException : Exception
    {
        public ProductConcurrencyException()
            : base(
                "The product was modified by another request. Please reload the product and try again.")
        {
        }
    }
}
