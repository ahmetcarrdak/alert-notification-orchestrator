namespace AlertNotificationService.Application.Common.Interfaces;

public interface IWatchdogTracker
{
    void RecordHeartbeat(string jobName);
    IReadOnlyDictionary<string, DateTime> GetAllHeartbeats();
    bool IsAlertSent(string jobName);
    void MarkAlertSent(string jobName);
    void ClearAlertSent(string jobName);
}
