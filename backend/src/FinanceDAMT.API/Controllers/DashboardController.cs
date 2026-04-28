using FinanceDAMT.Application.Features.Dashboard.DTOs;
using FinanceDAMT.Application.Features.Dashboard.Queries.GetDashboardSummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceDAMT.API.Controllers;

/// <summary>
/// Endpoints for dashboard analytics and projections.
/// </summary>
[ApiController]
[Authorize]
[Route("api/dashboard")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardController"/> class.
    /// </summary>
    /// <param name="mediator">MediatR request dispatcher.</param>
    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Returns the dashboard summary for the requested month and year.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard([FromQuery] int? month = null, [FromQuery] int? year = null, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var result = await _mediator.Send(new GetDashboardSummaryQuery(month ?? now.Month, year ?? now.Year), ct);
        return Ok(result);
    }
}
