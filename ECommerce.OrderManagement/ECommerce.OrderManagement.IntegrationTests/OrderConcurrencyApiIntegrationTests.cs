using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.IntegrationTests
{
    public sealed class OrderConcurrencyApiIntegrationTests
    {
        [Fact]
        public async Task CancelOrder_WhenConcurrencyExceptionOccurs_Returns409ProblemDetails()
        {
            using var factory =
                new ConcurrencyExceptionWebApplicationFactory();

            using var client = factory.CreateClient();

 
        }
    }
}
