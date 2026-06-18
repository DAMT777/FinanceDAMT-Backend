using FinanceDAMT.Application.Features.Subscriptions.Commands.CreateSubscription;
using FinanceDAMT.Application.Features.Subscriptions.Commands.DeleteSubscription;
using FinanceDAMT.Application.Features.Subscriptions.Commands.UpdateSubscription;
using FinanceDAMT.Application.Features.Subscriptions.DTOs;
using FinanceDAMT.Application.Features.Subscriptions.Queries.GetSubscriptionById;
using FinanceDAMT.Application.Features.Subscriptions.Queries.GetSubscriptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceDAMT.API.Controllers;

/// <summary>
/// Endpoints for managing user subscriptions (recurring services).
/// </summary>
[ApiController]
[Authorize]
[Route("api/subscriptions")]
[Produces("application/json")]
public class SubscriptionsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubscriptionsController"/> class.
    /// </summary>
    /// <param name="mediator">MediatR request dispatcher.</param>
    public SubscriptionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Returns all subscriptions for the current user, ordered by next billing date.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SubscriptionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubscriptions(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSubscriptionsQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Returns a single subscription by identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubscriptionById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSubscriptionByIdQuery(id), ct);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new subscription.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSubscriptionRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreateSubscriptionCommand(
                request.Name,
                request.Amount,
                request.BillingCycle,
                request.NextBillingDate,
                request.Icon,
                request.Notes),
            ct);

        return CreatedAtAction(nameof(GetSubscriptionById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing subscription.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSubscriptionRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new UpdateSubscriptionCommand(
                id,
                request.Name,
                request.Amount,
                request.BillingCycle,
                request.NextBillingDate,
                request.Icon,
                request.IsActive,
                request.Notes),
            ct);

        return Ok(result);
    }

    /// <summary>
    /// Soft deletes a subscription.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteSubscriptionCommand(id), ct);
        return NoContent();
    }
}
