using AlertNotificationService.Application.Common.DTOs;

namespace AlertNotificationService.Application.Common.Interfaces;

public interface ITeamsNotificationSender
{
    Task SendAsync(
        AlertmanagerPayloadDto payload,
        IReadOnlyDictionary<string, string> logsByFingerprint,
        CancellationToken cancellationToken = default);
}
