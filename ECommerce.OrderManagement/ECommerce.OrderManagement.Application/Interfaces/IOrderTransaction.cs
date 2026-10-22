using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application.Interfaces
{
    public interface IOrderTransaction
    {
        Task ExecuteAsync(
            Func<CancellationToken, Task> operation,
            CancellationToken cancellationToken);
    }
}
