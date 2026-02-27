namespace AlertNotificationService.Application.Common.DTOs;

public class AlertmanagerPayloadDto
{
    public string Version { get; set; } = string.Empty;
    public string GroupKey { get; set; } = string.Empty;
    public int TruncatedAlerts { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Receiver { get; set; } = string.Empty;
    public Dictionary<string, string> GroupLabels { get; set; } = new();
    public Dictionary<string, string> CommonLabels { get; set; } = new();
    public Dictionary<string, string> CommonAnnotations { get; set; } = new();
    public string ExternalURL { get; set; } = string.Empty;
    public List<AlertItemDto> Alerts { get; set; } = new();
}

public class AlertItemDto
{
    public string Status { get; set; } = string.Empty;
    public Dictionary<string, string> Labels { get; set; } = new();
    public Dictionary<string, string> Annotations { get; set; } = new();
    public DateTime StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public string GeneratorURL { get; set; } = string.Empty;
    public string Fingerprint { get; set; } = string.Empty;
}
