using AlertNotificationService.Domain.Interfaces;
using MediatR;

namespace AlertNotificationService.Application.Alerts.Queries.GetAlerts;

public class GetAlertsQueryHandler : IRequestHandler<GetAlertsQuery, IEnumerable<AlertResponse>>
{
    private readonly IAlertRepository _alertRepository;

    public GetAlertsQueryHandler(IAlertRepository alertRepository)
    {
        _alertRepository = alertRepository;
    }

    public async Task<IEnumerable<AlertResponse>> Handle(GetAlertsQuery request, CancellationToken cancellationToken)
    {
        var alerts = await _alertRepository.GetAllAsync(cancellationToken);

        return alerts.Select(a => new AlertResponse
        {
            Id = a.Id,
            AlertName = a.AlertName,
            Severity = a.Severity,
            Status = a.Status.ToString(),
            Summary = a.Summary,
            Instance = a.Instance,
            Job = a.Job,
            Fingerprint = a.Fingerprint,
            StartsAt = a.StartsAt,
            EndsAt = a.EndsAt,
            CreatedAt = a.CreatedAt
        });
    }
}
