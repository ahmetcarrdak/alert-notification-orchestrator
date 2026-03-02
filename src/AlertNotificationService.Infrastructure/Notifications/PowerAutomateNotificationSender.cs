using System.Text;
using System.Text.Json;
using AlertNotificationService.Application.Common.DTOs;
using AlertNotificationService.Application.Common.Interfaces;
using AlertNotificationService.Domain.Exceptions;
using AlertNotificationService.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AlertNotificationService.Infrastructure.Notifications;

public class PowerAutomateNotificationSender : ITeamsNotificationSender
{
    private readonly HttpClient _httpClient;
    private readonly TeamsSettings _settings;
    private readonly ILogger<PowerAutomateNotificationSender> _logger;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public PowerAutomateNotificationSender(
        HttpClient httpClient,
        IOptions<TeamsSettings> settings,
        ILogger<PowerAutomateNotificationSender> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(
        AlertmanagerPayloadDto payload,
        IReadOnlyDictionary<string, string> logsByFingerprint,
        CancellationToken cancellationToken = default)
    {
        var teamsPayload = BuildPayload(payload, logsByFingerprint);
        var json = JsonSerializer.Serialize(teamsPayload, SerializerOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_settings.WebhookUrl, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Teams notification failed. StatusCode: {StatusCode}, Body: {Body}",
                (int)response.StatusCode, body);

            throw new DomainException($"Teams notification failed with status {(int)response.StatusCode}.");
        }

        _logger.LogInformation(
            "Teams notification sent. Status: {Status}, AlertCount: {Count}",
            payload.Status, payload.Alerts.Count);
    }

    public async Task SendWatchdogAlertAsync(
        string jobName,
        DateTime lastSeen,
        CancellationToken cancellationToken = default)
    {
        var card = new
        {
            type    = "AdaptiveCard",
            version = "1.2",
            body    = new List<object>
            {
                new
                {
                    type   = "TextBlock",
                    text   = "⚠️ SERVIS YANIT VERMIYOR",
                    size   = "Large",
                    weight = "Bolder",
                    color  = "Attention",
                    wrap   = true
                },
                new
                {
                    type  = "Container",
                    style = "attention",
                    items = new List<object>
                    {
                        new
                        {
                            type = "FactSet",
                            facts = new List<object>
                            {
                                new { title = "Servis",          value = jobName },
                                new { title = "Son Sinyal",      value = $"{lastSeen:yyyy-MM-dd HH:mm:ss} UTC" },
                                new { title = "Sessiz Suresi",   value = $"{(DateTime.UtcNow - lastSeen).TotalMinutes:F0} dakika" }
                            }
                        }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(card, SerializerOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_settings.WebhookUrl, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Watchdog Teams alert failed. StatusCode: {StatusCode}, Body: {Body}",
                (int)response.StatusCode, body);
            return;
        }

        _logger.LogWarning("Watchdog alert sent to Teams for {Job}", jobName);
    }

    private static object BuildPayload(AlertmanagerPayloadDto payload, IReadOnlyDictionary<string, string> logsByFingerprint)
    {
        var isFiring   = payload.Status.Equals("firing", StringComparison.OrdinalIgnoreCase);
        var statusIcon = isFiring ? "🔴" : "🟢";
        var statusText = isFiring ? "YENİ HATA" : "HATA ÇÖZÜLDÜ";

        var bodyItems = new List<object>
        {
            new
            {
                type   = "TextBlock",
                text   = $"{statusIcon} {statusText} — {payload.Alerts.Count} alert(s)",
                size   = "Large",
                weight = "Bolder",
                color  = isFiring ? "Attention" : "Good",
                wrap   = true
            }
        };

        foreach (var a in payload.Alerts)
        {
            var httpStatus    = a.Labels.GetValueOrDefault("http_response_status_code", string.Empty);
            var method        = a.Labels.GetValueOrDefault("http_request_method", string.Empty);
            var route         = a.Labels.GetValueOrDefault("http_route", string.Empty);
            var service       = a.Labels.GetValueOrDefault("job", string.Empty);
            var alertName     = a.Labels.GetValueOrDefault("alertname", "Unknown");
            var desc          = a.Annotations.GetValueOrDefault("description", string.Empty);
            var summary       = a.Annotations.GetValueOrDefault("summary", string.Empty);
            var alertIsFiring = a.Status.Equals("firing", StringComparison.OrdinalIgnoreCase);

            var endpoint = (method, route) switch
            {
                ({ Length: > 0 }, { Length: > 0 }) => $"{method} /{route}",
                (_, { Length: > 0 })               => $"/{route}",
                _                                  => string.Empty
            };

            var facts = new List<object>();
            if (!string.IsNullOrEmpty(httpStatus)) facts.Add(new { title = "HTTP Status", value = httpStatus });
            if (!string.IsNullOrEmpty(service))    facts.Add(new { title = "Servis",      value = service });
            if (!string.IsNullOrEmpty(endpoint))   facts.Add(new { title = "Endpoint",    value = endpoint });
            facts.Add(new { title = "Tarih", value = $"{a.StartsAt:yyyy-MM-dd HH:mm:ss} UTC" });

            var alertDescription = !string.IsNullOrEmpty(desc) ? desc : summary;

            var containerItems = new List<object>
            {
                new { type = "TextBlock", text = $"**{alertName}**", weight = "Bolder", wrap = true },
                new { type = "FactSet", facts }
            };

            if (!string.IsNullOrEmpty(alertDescription))
            {
                containerItems.Add(new
                {
                    type  = "TextBlock",
                    text  = $"**Hata Açıklaması:** {alertDescription}",
                    wrap  = true,
                    size  = "Small"
                });
            }

            logsByFingerprint.TryGetValue(a.Fingerprint, out var seqLogs);
            if (!string.IsNullOrEmpty(seqLogs))
            {
                containerItems.Add(new
                {
                    type    = "ActionSet",
                    actions = new List<object>
                    {
                        new
                        {
                            type  = "Action.ShowCard",
                            title = "Hata Logu",
                            card  = new
                            {
                                type = "AdaptiveCard",
                                body = new List<object>
                                {
                                    new { type = "TextBlock", text = seqLogs, wrap = true, fontType = "Monospace" }
                                }
                            }
                        }
                    }
                });
            }

            bodyItems.Add(new
            {
                type  = "Container",
                style = alertIsFiring ? "attention" : "good",
                items = containerItems
            });
        }

        return new
        {
            type    = "AdaptiveCard",
            version = "1.2",
            body    = bodyItems
        };
    }
}
