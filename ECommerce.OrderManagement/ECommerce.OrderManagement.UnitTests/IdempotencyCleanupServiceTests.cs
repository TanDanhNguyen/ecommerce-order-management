using ECommerce.OrderManagement.Application.Interfaces;
using ECommerce.OrderManagement.Infrastructure.BackgroundServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.OrderManagement.UnitTests
{
    public class IdempotencyCleanupServiceTests
    {


        [Fact]
        public async Task StartAsync_CallsDeleteExpiredAsync()
        {
            // Arrange
            var cleanupCalled = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            var cleanupMock = new Mock<IIdempotencyCleanup>();

            cleanupMock
                .Setup(x => x.CleanupExpiredAsync(
                    It.IsAny<CancellationToken>()))
                .Returns<CancellationToken>(_ =>
                {
                    cleanupCalled.TrySetResult(true);
                    return Task.FromResult(2);
                });

            var services = new ServiceCollection();
            services.AddSingleton(cleanupMock.Object);

            using var serviceProvider = services.BuildServiceProvider();

            var loggerMock = new Mock<ILogger<IdempotencyCleanupService>>();

            using var service = new IdempotencyCleanupService(
                serviceProvider.GetRequiredService<IServiceScopeFactory>(),
                loggerMock.Object);

            // Act
            await service.StartAsync(CancellationToken.None);

            await cleanupCalled.Task.WaitAsync(TimeSpan.FromSeconds(5));

            await service.StopAsync(CancellationToken.None);

            // Assert
            cleanupMock.Verify(
                x => x.CleanupExpiredAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }


        [Fact]
        public async Task StartAsync_WhenCleanupFails_LogsErrorAndContinues()
        {
            // Arrange
            var cleanupCalled = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            var cleanupMock = new Mock<IIdempotencyCleanup>();

            cleanupMock
                .Setup(x => x.CleanupExpiredAsync(
                    It.IsAny<CancellationToken>()))
                .Returns<CancellationToken>(cancellationToken =>
                {
                    cleanupCalled.TrySetResult(true);

                    return Task.FromException<int>(
                        new InvalidOperationException("Database error"));
                });

            var services = new ServiceCollection();
            services.AddSingleton(cleanupMock.Object);

            using var serviceProvider = services.BuildServiceProvider();

            var loggerMock = new Mock<ILogger<IdempotencyCleanupService>>();

            using var service = new IdempotencyCleanupService(
                serviceProvider.GetRequiredService<IServiceScopeFactory>(),
                loggerMock.Object);

            // Act
            await service.StartAsync(CancellationToken.None);

            await cleanupCalled.Task.WaitAsync(TimeSpan.FromSeconds(5));

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, _) =>
                        state.ToString()!.Contains(
                            "Failed to clean up expired idempotency records.")),
                    It.Is<Exception>(exception =>
                        exception is InvalidOperationException &&
                        exception.Message == "Database error"),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            await service.StopAsync(CancellationToken.None);
        }

    }
}
