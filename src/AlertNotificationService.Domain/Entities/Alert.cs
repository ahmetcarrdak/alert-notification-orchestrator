using AlertNotificationService.Domain.Enums;

namespace AlertNotificationService.Domain.Entities;

public class Alert : BaseEntity
{
    public string AlertName { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public AlertStatus Status { get; set; }
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public string? Instance { get; set; }
    public string? Job { get; set; }
    public string Fingerprint { get; set; } = string.Empty;
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public string LabelsJson { get; set; } = string.Empty;
}
