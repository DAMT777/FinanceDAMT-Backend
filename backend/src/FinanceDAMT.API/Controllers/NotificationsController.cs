using FinanceDAMT.Application.Features.Notifications.Commands.DeleteNotification;
using FinanceDAMT.Application.Features.Notifications.Commands.MarkAllAsRead;
using FinanceDAMT.Application.Features.Notifications.Commands.MarkAsRead;
using FinanceDAMT.Application.Features.Notifications.DTOs;
using FinanceDAMT.Application.Features.Notifications.Queries.GetNotifications;
using FinanceDAMT.Application.Features.Notifications.Queries.GetUnreadCount;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceDAMT.API.Controllers;

/// <summary>
/// Endpoints for the current user's in-app notifications.
/// </summary>
[ApiController]
[Authorize]
[Route("api/notifications")]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationsController"/> class.
    /// </summary>
    /// <param name="mediator">MediatR request dispatcher.</param>
    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Returns the latest notifications for the current user (newest first).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetNotificationsQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Returns the number of unread notifications (for badges).
    /// </summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct)
    {
        var count = await _mediator.Send(new GetUnreadNotificationCountQuery(), ct);
        return Ok(new { count });
    }

    /// <summary>
    /// Marks a single notification as read.
    /// </summary>
    [HttpPost("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new MarkNotificationAsReadCommand(id), ct);
        return NoContent();
    }

    /// <summary>
    /// Marks every unread notification as read.
    /// </summary>
    [HttpPost("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct)
    {
        await _mediator.Send(new MarkAllNotificationsAsReadCommand(), ct);
        return NoContent();
    }

    /// <summary>
    /// Soft deletes a notification.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteNotificationCommand(id), ct);
        return NoContent();
    }
}
