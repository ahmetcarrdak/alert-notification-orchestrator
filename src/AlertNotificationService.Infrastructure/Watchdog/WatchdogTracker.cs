using System.Collections.Concurrent;
using AlertNotificationService.Application.Common.Interfaces;

namespace AlertNotificationService.Infrastructure.Watchdog;

public class WatchdogTracker : IWatchdogTracker
{
    private readonly ConcurrentDictionary<string, DateTime> _heartbeats = new();
    private readonly ConcurrentDictionary<string, bool> _alertsSent = new();

    public void RecordHeartbeat(string jobName)
        => _heartbeats[jobName] = DateTime.UtcNow;

    public IReadOnlyDictionary<string, DateTime> GetAllHeartbeats()
        => _heartbeats;

    public bool IsAlertSent(string jobName)
        => _alertsSent.ContainsKey(jobName);

    public void MarkAlertSent(string jobName)
        => _alertsSent[jobName] = true;

    public void ClearAlertSent(string jobName)
        => _alertsSent.TryRemove(jobName, out _);
}
