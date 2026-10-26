using ECommerce.OrderManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.UnitTests
{
    public class IdempotencyRecordTests
    {
        [Fact]
        public void Constructor_ExpirationLaterThanCreation_CreatesRecord()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var createdAt = DateTime.UtcNow;
            var expiresAt = createdAt.AddHours(24);

            // Act
            var record = new IdempotencyRecord(
                userId,
                "test-key",
                new string('a', 64),
                201,
                """{"orderId":"test"}""",
                createdAt,
                expiresAt);

            // Assert
            Assert.Equal(userId, record.UserId);
            Assert.Equal("test-key", record.Key);
            Assert.Equal(createdAt, record.CreatedAtUtc);
            Assert.Equal(expiresAt, record.ExpiresAtUtc);
        }

        [Fact]
        public void Constructor_ExpirationNotLaterThanCreation_ThrowsArgumentException()
        {
            // Arrange
            var createdAt = DateTime.UtcNow;

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                new IdempotencyRecord(
                    Guid.NewGuid(),
                    "test-key",
                    new string('a', 64),
                    201,
                    """{"orderId":"test"}""",
                    createdAt,
                    createdAt.AddHours(-1)));
        }
    }
}
