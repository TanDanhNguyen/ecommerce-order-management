using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.IntegrationTests.Orders
{
    public class IdempotencyTests
    {
        [Fact]
        public async Task CreateOrder_SameIdempotencyKey_ReturnsSameOrder()
        {
            // Arrange

            // Act
            // Gửi POST /api/Orders hai lần
            // Dùng cùng Idempotency-Key và cùng request body

            // Assert
            // Cả hai response thành công
            // Cùng Order ID
            // Chỉ có một Order được tạo
            // Stock chỉ giảm một lần
        }
    }
}
