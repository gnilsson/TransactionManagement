using API.Data;
using API.Misc;

namespace API.Features.Auditing;

public sealed class AuditingBackgroundService : BackgroundService
{
    private readonly IBackgroundTaskQueueReader<IEnumerable<AuditLog>> _queue;
    private readonly ILogger<AuditingBackgroundService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public AuditingBackgroundService(
        IBackgroundTaskQueueReader<IEnumerable<AuditLog>> queue,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<AuditingBackgroundService> logger)
    {
        _queue = queue;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformOperationAsync(stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while processing the audit logs.");
                throw;
            }
        }
    }

    private async Task PerformOperationAsync(CancellationToken stoppingToken)
    {
        if (await _queue.WaitToReadAsync(stoppingToken))
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();

            var auditLogs = await _queue.DequeueAsync(stoppingToken);

            dbContext.AuditLogs.AddRange(auditLogs);
            await dbContext.SaveChangesAsync(stoppingToken);
        }
    }
}
