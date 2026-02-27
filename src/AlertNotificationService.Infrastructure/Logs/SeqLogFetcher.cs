using System.Text;
using System.Text.Json;
using AlertNotificationService.Application.Common.Interfaces;
using AlertNotificationService.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AlertNotificationService.Infrastructure.Logs;

public class SeqLogFetcher : ISeqLogFetcher
{
    private readonly HttpClient _httpClient;
    private readonly SeqSettings _settings;
    private readonly ILogger<SeqLogFetcher> _logger;

    public SeqLogFetcher(
        HttpClient httpClient,
        IOptions<SeqSettings> settings,
        ILogger<SeqLogFetcher> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string> GetRecentErrorLogsAsync(string jobName, DateTime from, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_settings.Url))
            return string.Empty;

        var appName = JobNameToAppName(jobName);
        var fromUtc = from.ToUniversalTime().AddMinutes(-2);
        var filter = $"Application = '{appName}' and @Level in ['Error', 'Warning']";

        var url = $"{_settings.Url.TrimEnd('/')}/api/events?clef" +
                  $"&filter={Uri.EscapeDataString(filter)}" +
                  $"&fromDateUtc={fromUtc:O}" +
                  $"&count=5";

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("SEQ query returned {Status} for job {Job}", (int)response.StatusCode, jobName);
                return string.Empty;
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return FormatLogs(body);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch logs from SEQ for job {Job}", jobName);
            return string.Empty;
        }
    }

    private static string JobNameToAppName(string jobName) =>
        string.Join(' ', jobName.Split('-').Select(w => w.Length > 0 ? char.ToUpperInvariant(w[0]) + w[1..] : w));

    private static string FormatLogs(string clefBody)
    {
        if (string.IsNullOrWhiteSpace(clefBody))
            return string.Empty;

        var sb = new StringBuilder();
        foreach (var line in clefBody.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            try
            {
                using var doc = JsonDocument.Parse(line.Trim());
                var root = doc.RootElement;

                var timestamp = root.TryGetProperty("@t", out var t) ? t.GetString() : null;
                var level     = root.TryGetProperty("@l", out var l) ? l.GetString() : "Information";
                var message   = root.TryGetProperty("@mt", out var mt) ? mt.GetString()
                              : root.TryGetProperty("@m",  out var m)  ? m.GetString()
                              : null;
                var exception = root.TryGetProperty("@x", out var x) ? x.GetString() : null;

                if (timestamp != null && DateTime.TryParse(timestamp, out var dt))
                    sb.Append($"[{dt:HH:mm:ss}] ");
                if (level    != null) sb.Append($"[{level}] ");
                if (message  != null) sb.AppendLine(message);

                if (!string.IsNullOrEmpty(exception))
                    sb.AppendLine(exception.Split('\n').FirstOrDefault());
            }
            catch { /* skip malformed lines */ }
        }

        return sb.ToString().Trim();
    }
}
