namespace API.Misc;

public abstract class MonitoredBackgroundService : BackgroundService
{
    private readonly ILogger<MonitoredBackgroundService> _logger;

    protected MonitoredBackgroundService(ILogger<MonitoredBackgroundService> logger)
    {
        _logger = logger;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{Service}: Starting.", ServiceName);
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{Service}: Stopping.", ServiceName);
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await PerformOperationAsync(stoppingToken);
            }
        }
        catch (Exception e) when (e is OperationCanceledException or TaskCanceledException)
        {
            _logger.LogInformation("{Service}: Cancellation requested. Stopping the background service.", ServiceName);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{Service}: An error occurred while processing.", ServiceName);
            throw;
        }
        finally
        {
            _logger.LogInformation("{Service}: Completed processing.", ServiceName);
        }
    }

    protected abstract string ServiceName { get; }

    protected abstract Task PerformOperationAsync(CancellationToken stoppingToken);
}
