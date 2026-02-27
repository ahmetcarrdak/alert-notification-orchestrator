using FluentValidation;

namespace AlertNotificationService.Application.Alerts.Commands.ProcessAlertWebhook;

public class ProcessAlertWebhookCommandValidator : AbstractValidator<ProcessAlertWebhookCommand>
{
    public ProcessAlertWebhookCommandValidator()
    {
        RuleFor(x => x.Payload)
            .NotNull()
            .WithMessage("Payload cannot be null.");

        When(x => x.Payload is not null, () =>
        {
            RuleFor(x => x.Payload.Status)
                .NotEmpty()
                .WithMessage("Status is required.");

            RuleFor(x => x.Payload.Alerts)
                .NotEmpty()
                .WithMessage("Alerts list cannot be empty.");
        });
    }
}
