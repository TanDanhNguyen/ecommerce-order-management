using ECommerce.OrderManagement.Infrastructure.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.IntegrationTests
{
    public class TestDatabaseIsolationTests
    {
        [Fact]
        public async Task VerifyTestDatabases_UsesSeparateTestDatabases()
        {
            await using var factory = new CustomWebApplicationFactory();

            await factory.VerifyTestDatabasesAsync();
        }
    }
}
