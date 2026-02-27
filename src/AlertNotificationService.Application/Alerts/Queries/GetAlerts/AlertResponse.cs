namespace AlertNotificationService.Application.Alerts.Queries.GetAlerts;

public class AlertResponse
{
    public Guid Id { get; set; }
    public string AlertName { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Instance { get; set; }
    public string? Job { get; set; }
    public string Fingerprint { get; set; } = string.Empty;
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
