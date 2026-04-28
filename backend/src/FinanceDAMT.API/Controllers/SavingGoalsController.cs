using FinanceDAMT.Application.Features.SavingGoals.Commands.AddContribution;
using FinanceDAMT.Application.Features.SavingGoals.Commands.CreateSavingGoal;
using FinanceDAMT.Application.Features.SavingGoals.Commands.DeleteSavingGoal;
using FinanceDAMT.Application.Features.SavingGoals.Commands.RemoveContribution;
using FinanceDAMT.Application.Features.SavingGoals.Commands.UpdateSavingGoal;
using FinanceDAMT.Application.Features.SavingGoals.DTOs;
using FinanceDAMT.Application.Features.SavingGoals.Queries.GetSavingGoalById;
using FinanceDAMT.Application.Features.SavingGoals.Queries.GetSavingGoals;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceDAMT.API.Controllers;

/// <summary>
/// Endpoints for saving goals and contributions.
/// </summary>
[ApiController]
[Authorize]
[Route("api/saving-goals")]
[Produces("application/json")]
public class SavingGoalsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="SavingGoalsController"/> class.
    /// </summary>
    /// <param name="mediator">MediatR request dispatcher.</param>
    public SavingGoalsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Returns all saving goals for current user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SavingGoalDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSavingGoals(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSavingGoalsQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Returns a single saving goal including contribution history.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SavingGoalDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSavingGoalById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSavingGoalByIdQuery(id), ct);
        return Ok(result);
    }

    /// <summary>
    /// Creates a saving goal.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SavingGoalDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateSavingGoalRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateSavingGoalCommand(request.Name, request.TargetAmount, request.Deadline, request.Icon), ct);
        return CreatedAtAction(nameof(GetSavingGoalById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing saving goal.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(SavingGoalDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSavingGoalRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateSavingGoalCommand(id, request.Name, request.TargetAmount, request.Deadline, request.Icon), ct);
        return Ok(result);
    }

    /// <summary>
    /// Soft deletes a saving goal.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteSavingGoalCommand(id), ct);
        return NoContent();
    }

    /// <summary>
    /// Adds a contribution to a saving goal.
    /// </summary>
    [HttpPost("{id:guid}/contributions")]
    [ProducesResponseType(typeof(SavingGoalDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddContribution(Guid id, [FromBody] AddContributionRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new AddContributionCommand(id, request.Amount, request.Date, request.Note), ct);
        return Ok(result);
    }

    /// <summary>
    /// Removes a contribution from a saving goal.
    /// </summary>
    [HttpDelete("{id:guid}/contributions/{cid:guid}")]
    [ProducesResponseType(typeof(SavingGoalDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveContribution(Guid id, Guid cid, CancellationToken ct)
    {
        var result = await _mediator.Send(new RemoveContributionCommand(id, cid), ct);
        return Ok(result);
    }
}
