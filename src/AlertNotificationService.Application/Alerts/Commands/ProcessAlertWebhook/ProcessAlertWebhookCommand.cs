using AlertNotificationService.Application.Common.DTOs;
using MediatR;

namespace AlertNotificationService.Application.Alerts.Commands.ProcessAlertWebhook;

public record ProcessAlertWebhookCommand(AlertmanagerPayloadDto Payload) : IRequest<Unit>;
