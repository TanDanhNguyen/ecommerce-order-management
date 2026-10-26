using ECommerce.OrderManagement.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerce.OrderManagement.Infrastructure.BackgroundServices
{
    public sealed class IdempotencyCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<IdempotencyCleanupService> _logger;

        public IdempotencyCleanupService(
            IServiceScopeFactory scopeFactory,
            ILogger<IdempotencyCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromHours(1));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredRecordsAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    _logger.LogError(
                        exception,
                        "Failed to clean up expired idempotency records.");
                }

                try
                {
                    if (!await timer.WaitForNextTickAsync(stoppingToken))
                        break;
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        private async Task CleanupExpiredRecordsAsync(
            CancellationToken cancellationToken)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var cleanup = scope.ServiceProvider
                .GetRequiredService<IIdempotencyCleanup>();

            var deletedCount = await cleanup.CleanupExpiredAsync(
                cancellationToken);

            if (deletedCount > 0)
            {
                _logger.LogInformation(
                    "Deleted {Count} expired idempotency records.",
                    deletedCount);
            }
        }
    }
}
