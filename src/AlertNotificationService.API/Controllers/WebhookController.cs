using AlertNotificationService.Application.Alerts.Commands.ProcessAlertWebhook;
using AlertNotificationService.Application.Alerts.Commands.ProcessWatchdog;
using AlertNotificationService.Application.Common.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AlertNotificationService.API.Controllers;

[ApiController]
[Route("webhook")]
public class WebhookController : ControllerBase
{
    private readonly IMediator _mediator;

    public WebhookController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("alertmanager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReceiveAlertmanagerWebhook(
        [FromBody] AlertmanagerPayloadDto payload,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new ProcessAlertWebhookCommand(payload), cancellationToken);
        return Ok();
    }

    [HttpPost("watchdog")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReceiveWatchdog(
        [FromBody] AlertmanagerPayloadDto payload,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new ProcessWatchdogCommand(payload), cancellationToken);
        return Ok();
    }
}
