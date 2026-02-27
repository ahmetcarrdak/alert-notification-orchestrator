using System.Text.Json;
using AlertNotificationService.Application.Common.DTOs;
using AlertNotificationService.Application.Common.Interfaces;
using AlertNotificationService.Domain.Entities;
using AlertNotificationService.Domain.Enums;
using AlertNotificationService.Domain.Interfaces;
using MediatR;

namespace AlertNotificationService.Application.Alerts.Commands.ProcessAlertWebhook;

public class ProcessAlertWebhookCommandHandler : IRequestHandler<ProcessAlertWebhookCommand, Unit>
{
    private readonly IAlertRepository _alertRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITeamsNotificationSender _teamsSender;
    private readonly ISeqLogFetcher _seqLogFetcher;

    public ProcessAlertWebhookCommandHandler(
        IAlertRepository alertRepository,
        IUnitOfWork unitOfWork,
        ITeamsNotificationSender teamsSender,
        ISeqLogFetcher seqLogFetcher)
    {
        _alertRepository = alertRepository;
        _unitOfWork = unitOfWork;
        _teamsSender = teamsSender;
        _seqLogFetcher = seqLogFetcher;
    }

    public async Task<Unit> Handle(ProcessAlertWebhookCommand request, CancellationToken cancellationToken)
    {
        foreach (var alertItem in request.Payload.Alerts)
        {
            var status = alertItem.Status.Equals("resolved", StringComparison.OrdinalIgnoreCase)
                ? AlertStatus.Resolved
                : AlertStatus.Firing;

            if (status == AlertStatus.Resolved)
            {
                var existing = await _alertRepository.GetFiringByFingerprintAsync(alertItem.Fingerprint, cancellationToken);
                if (existing is not null)
                {
                    existing.Status = AlertStatus.Resolved;
                    existing.EndsAt = alertItem.EndsAt;
                    existing.UpdatedAt = DateTime.UtcNow;
                    await _alertRepository.UpdateAsync(existing, cancellationToken);
                }
                else
                {
                    await _alertRepository.AddAsync(MapToEntity(alertItem, status), cancellationToken);
                }
            }
            else
            {
                await _alertRepository.AddAsync(MapToEntity(alertItem, status), cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var logsByFingerprint = new Dictionary<string, string>();
        foreach (var alertItem in request.Payload.Alerts)
        {
            var job = alertItem.Labels.GetValueOrDefault("job", string.Empty);
            if (string.IsNullOrEmpty(job)) continue;

            var logs = await _seqLogFetcher.GetRecentErrorLogsAsync(job, alertItem.StartsAt, cancellationToken);
            if (!string.IsNullOrEmpty(logs))
                logsByFingerprint[alertItem.Fingerprint] = logs;
        }

        await _teamsSender.SendAsync(request.Payload, logsByFingerprint, cancellationToken);

        return Unit.Value;
    }

    private static Alert MapToEntity(AlertItemDto item, AlertStatus status) => new()
    {
        AlertName = item.Labels.GetValueOrDefault("alertname", "Unknown"),
        Severity = item.Labels.GetValueOrDefault("severity", "none"),
        Status = status,
        Summary = item.Annotations.GetValueOrDefault("summary"),
        Description = item.Annotations.GetValueOrDefault("description"),
        Instance = item.Labels.GetValueOrDefault("instance"),
        Job = item.Labels.GetValueOrDefault("job"),
        Fingerprint = item.Fingerprint,
        StartsAt = item.StartsAt,
        EndsAt = item.EndsAt,
        LabelsJson = JsonSerializer.Serialize(item.Labels)
    };
}
