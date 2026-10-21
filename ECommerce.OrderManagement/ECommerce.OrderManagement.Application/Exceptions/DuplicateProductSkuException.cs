using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application.Exceptions
{
    public sealed class DuplicateProductSkuException : Exception
    {
        public DuplicateProductSkuException(string sku)
            : base($"Product SKU '{sku}' already exists.")
        {
        }
    }
}
