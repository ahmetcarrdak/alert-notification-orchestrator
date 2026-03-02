using FluentValidation;

namespace AlertNotificationService.Application.Alerts.Commands.ProcessWatchdog;

public class ProcessWatchdogCommandValidator : AbstractValidator<ProcessWatchdogCommand>
{
    public ProcessWatchdogCommandValidator()
    {
        RuleFor(x => x.Payload).NotNull();
        RuleFor(x => x.Payload.Alerts).NotEmpty();
    }
}
