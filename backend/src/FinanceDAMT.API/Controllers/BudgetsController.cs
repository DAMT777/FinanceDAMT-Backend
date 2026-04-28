using FinanceDAMT.Application.Features.Budgets.Commands.DeleteBudget;
using FinanceDAMT.Application.Features.Budgets.Commands.SetBudget;
using FinanceDAMT.Application.Features.Budgets.DTOs;
using FinanceDAMT.Application.Features.Budgets.Queries.GetBudgets;
using FinanceDAMT.Application.Features.Budgets.Queries.GetBudgetStatus;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceDAMT.API.Controllers;

/// <summary>
/// Endpoints for setting and tracking monthly budgets.
/// </summary>
[ApiController]
[Authorize]
[Route("api/budgets")]
[Produces("application/json")]
public class BudgetsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="BudgetsController"/> class.
    /// </summary>
    /// <param name="mediator">MediatR request dispatcher.</param>
    public BudgetsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Returns budgets for the requested month and year (defaults to current month).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BudgetDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBudgets([FromQuery] int? month = null, [FromQuery] int? year = null, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var result = await _mediator.Send(new GetBudgetsQuery(month ?? now.Month, year ?? now.Year), ct);
        return Ok(result);
    }

    /// <summary>
    /// Returns budget status including spent amount and usage percentage.
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(IReadOnlyList<BudgetStatusDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBudgetStatus([FromQuery] int? month = null, [FromQuery] int? year = null, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var result = await _mediator.Send(new GetBudgetStatusQuery(month ?? now.Month, year ?? now.Year), ct);
        return Ok(result);
    }

    /// <summary>
    /// Creates or updates a budget for category and month/year.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BudgetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetBudget([FromBody] SetBudgetRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new SetBudgetCommand(request.CategoryId, request.MonthlyLimit, request.Month, request.Year), ct);
        return Ok(result);
    }

    /// <summary>
    /// Removes a budget by soft delete.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteBudgetCommand(id), ct);
        return NoContent();
    }
}
