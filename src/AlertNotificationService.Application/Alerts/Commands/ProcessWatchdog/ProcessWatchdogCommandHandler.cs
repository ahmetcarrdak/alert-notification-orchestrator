using AlertNotificationService.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AlertNotificationService.Application.Alerts.Commands.ProcessWatchdog;

public class ProcessWatchdogCommandHandler : IRequestHandler<ProcessWatchdogCommand, Unit>
{
    private readonly IWatchdogTracker _tracker;
    private readonly ILogger<ProcessWatchdogCommandHandler> _logger;

    public ProcessWatchdogCommandHandler(IWatchdogTracker tracker, ILogger<ProcessWatchdogCommandHandler> logger)
    {
        _tracker = tracker;
        _logger = logger;
    }

    public Task<Unit> Handle(ProcessWatchdogCommand request, CancellationToken cancellationToken)
    {
        foreach (var alert in request.Payload.Alerts)
        {
            var job = alert.Labels.GetValueOrDefault("job", string.Empty);
            if (string.IsNullOrEmpty(job)) continue;

            _tracker.RecordHeartbeat(job);
            _tracker.ClearAlertSent(job);

            _logger.LogDebug("Watchdog heartbeat received for {Job}", job);
        }

        return Task.FromResult(Unit.Value);
    }
}
