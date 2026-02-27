using MediatR;

namespace AlertNotificationService.Application.Alerts.Queries.GetAlerts;

public record GetAlertsQuery : IRequest<IEnumerable<AlertResponse>>;
