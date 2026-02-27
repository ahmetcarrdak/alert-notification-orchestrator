namespace AlertNotificationService.Application.Common.Interfaces;

public interface ISeqLogFetcher
{
    Task<string> GetRecentErrorLogsAsync(string jobName, DateTime from, CancellationToken cancellationToken = default);
}
