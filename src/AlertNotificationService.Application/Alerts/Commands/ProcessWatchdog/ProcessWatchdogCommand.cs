using AlertNotificationService.Application.Common.DTOs;
using MediatR;

namespace AlertNotificationService.Application.Alerts.Commands.ProcessWatchdog;

public record ProcessWatchdogCommand(AlertmanagerPayloadDto Payload) : IRequest<Unit>;
