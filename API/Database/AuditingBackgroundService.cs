using API.Data;
using API.Misc;

namespace API.Database;

public sealed class AuditingBackgroundService : MonitoredBackgroundService
{
    private readonly IBackgroundTaskQueueReader<IEnumerable<AuditLog>> _queue;
    private readonly ILogger<AuditingBackgroundService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public AuditingBackgroundService(
        IBackgroundTaskQueueReader<IEnumerable<AuditLog>> queue,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<AuditingBackgroundService> logger) : base(logger)
    {
        _queue = queue;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override string ServiceName { get; } = nameof(AuditingBackgroundService);

    protected override async Task PerformOperationAsync(CancellationToken stoppingToken)
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
