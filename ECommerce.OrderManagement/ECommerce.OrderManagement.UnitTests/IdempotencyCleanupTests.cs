using ECommerce.OrderManagement.Application.Interfaces;
using ECommerce.OrderManagement.Infrastructure.BackgroundServices;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.UnitTests
{
    public class IdempotencyCleanupTests
    {
        [Fact]
        public async Task CleanupExpiredAsync_CallsRepositoryWithUtcTime()
        {
            // Arrange
            var repositoryMock = new Mock<IIdempotencyRepository>();

            repositoryMock
                .Setup(x => x.DeleteExpiredAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(3);

            var cleanup = new IdempotencyCleanup(repositoryMock.Object);
            using var cts = new CancellationTokenSource();

            // Act
            var deletedCount = await cleanup.CleanupExpiredAsync(cts.Token);

            // Assert
            Assert.Equal(3, deletedCount);

            repositoryMock.Verify(
                x => x.DeleteExpiredAsync(
                    It.Is<DateTime>(date => date.Kind == DateTimeKind.Utc),
                    cts.Token),
                Times.Once);
        }

        [Fact]
        public async Task CleanupExpiredAsync_WhenRepositoryFails_PropagatesException()
        {
            // Arrange
            var repositoryMock = new Mock<IIdempotencyRepository>();

            repositoryMock
                .Setup(x => x.DeleteExpiredAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            var cleanup = new IdempotencyCleanup(repositoryMock.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => cleanup.CleanupExpiredAsync(CancellationToken.None));

            Assert.Equal("Database error", exception.Message);
        }
    }
}
