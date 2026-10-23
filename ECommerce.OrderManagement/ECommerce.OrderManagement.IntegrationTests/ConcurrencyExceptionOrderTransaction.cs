using ECommerce.OrderManagement.Application.Exceptions;
using ECommerce.OrderManagement.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.IntegrationTests
{
    public sealed class ConcurrencyExceptionOrderTransaction
        : IOrderTransaction
    {
        public Task ExecuteAsync(
            Func<CancellationToken, Task> operation,
            CancellationToken cancellationToken)
        {
            throw new OrderConcurrencyException();
        }
    }
}
