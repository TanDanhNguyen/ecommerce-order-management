using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.Domain.Entities
{
    public class IdempotencyRecord
    {
        public Guid Id { get; private set; }

        public string Key { get; private set; } = string.Empty;

        public string RequestHash { get; private set; } = string.Empty;

        public int StatusCode { get; private set; }

        public string ResponseBody { get; private set; } = string.Empty;

        public DateTime CreatedAtUtc { get; private set; }

        public DateTime ExpiresAtUtc { get; private set; }

        public Guid UserId { get; private set; }

        private IdempotencyRecord()
        {
            // Required by EF Core
        }

        public IdempotencyRecord(
            Guid userId,
            string key,
            string requestHash,
            int statusCode,
            string responseBody,
            DateTime createdAtUtc,
            DateTime expiresAtUtc
           )
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Idempotency key is required.", nameof(key));

            if (string.IsNullOrWhiteSpace(requestHash))
                throw new ArgumentException("Request hash is required.", nameof(requestHash));

            if (expiresAtUtc <= createdAtUtc)
                throw new ArgumentException("Expiration must be later than creation.");

            Id = Guid.NewGuid();
            Key = key;
            RequestHash = requestHash;
            StatusCode = statusCode;
            ResponseBody = responseBody;
            CreatedAtUtc = createdAtUtc;
            ExpiresAtUtc = expiresAtUtc;
            UserId = userId;
        }
    }
}
