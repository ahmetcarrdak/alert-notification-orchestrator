using AlertNotificationService.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AlertNotificationService.Infrastructure.Watchdog;

public class WatchdogMonitorService : BackgroundService
{
    private readonly IWatchdogTracker _tracker;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WatchdogMonitorService> _logger;

    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan StaleThreshold = TimeSpan.FromMinutes(5);

    public WatchdogMonitorService(
        IWatchdogTracker tracker,
        IServiceScopeFactory scopeFactory,
        ILogger<WatchdogMonitorService> logger)
    {
        _tracker = tracker;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Watchdog monitor started. Check interval: {Interval}s, threshold: {Threshold}m",
            CheckInterval.TotalSeconds, StaleThreshold.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(CheckInterval, stoppingToken);

            var now = DateTime.UtcNow;

            foreach (var (job, lastSeen) in _tracker.GetAllHeartbeats())
            {
                var elapsed = now - lastSeen;

                if (elapsed <= StaleThreshold || _tracker.IsAlertSent(job))
                    continue;

                _logger.LogWarning("Watchdog stale for {Job}. Last heartbeat: {LastSeen} ({Elapsed} ago)",
                    job, lastSeen, elapsed);

                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var teamsSender = scope.ServiceProvider.GetRequiredService<ITeamsNotificationSender>();
                    await teamsSender.SendWatchdogAlertAsync(job, lastSeen, stoppingToken);
                    _tracker.MarkAlertSent(job);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send watchdog alert for {Job}", job);
                }
            }
        }
    }
}
