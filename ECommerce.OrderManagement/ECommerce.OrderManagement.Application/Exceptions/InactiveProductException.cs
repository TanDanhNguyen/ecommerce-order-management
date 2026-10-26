using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application.Exceptions
{
    public sealed class InactiveProductException : Exception
    {
        public InactiveProductException(string productName)
            : base($"Product '{productName}' is inactive and cannot be adjusted.")
        {
        }
    }
}
