using AlertNotificationService.Application.Alerts.Queries.GetAlerts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AlertNotificationService.API.Controllers;

[ApiController]
[Route("api/alerts")]
public class AlertsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AlertsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AlertResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAlerts(CancellationToken cancellationToken)
    {
        var alerts = await _mediator.Send(new GetAlertsQuery(), cancellationToken);
        return Ok(alerts);
    }
}
