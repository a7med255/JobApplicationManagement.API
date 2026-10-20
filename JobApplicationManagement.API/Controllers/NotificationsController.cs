using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Notifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobApplicationManagement.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;
    public NotificationsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<NotificationDto>>> GetMine(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default) =>
        Ok(await _mediator.Send(new GetMyNotificationsQuery(pageNumber, pageSize), cancellationToken));

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken) =>
        Ok(new { count = await _mediator.Send(new GetUnreadNotificationCountQuery(), cancellationToken) });

    [HttpPut("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new MarkNotificationReadCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        await _mediator.Send(new MarkAllNotificationsReadCommand(), cancellationToken);
        return NoContent();
    }

    [HttpGet("preferences")]
    public async Task<ActionResult<NotificationPreferenceDto>> GetPreferences(CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new GetMyNotificationPreferencesQuery(), cancellationToken));

    [HttpPut("preferences")]
    public async Task<ActionResult<NotificationPreferenceDto>> UpdatePreferences(
        NotificationPreferenceDto preferences, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new UpdateNotificationPreferencesCommand(preferences), cancellationToken));
}
