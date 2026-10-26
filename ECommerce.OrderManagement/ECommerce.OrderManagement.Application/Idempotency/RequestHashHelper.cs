using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Application.Idempotency
{
    public static class RequestHashHelper
    {
        public static string Compute<T>(T request)
        {
            var json = JsonSerializer.Serialize(request);
            var bytes = Encoding.UTF8.GetBytes(json);
            var hash = SHA256.HashData(bytes);

            return Convert.ToHexString(hash);
        }
    }
}
