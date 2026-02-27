namespace AlertNotificationService.Infrastructure.Settings;

public class TeamsSettings
{
    public const string SectionName = "Teams";
    public string WebhookUrl { get; set; } = string.Empty;
}
