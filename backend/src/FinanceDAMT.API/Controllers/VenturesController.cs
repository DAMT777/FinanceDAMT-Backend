using FinanceDAMT.Application.Features.Ventures.Commands.AddBatch;
using FinanceDAMT.Application.Features.Ventures.Commands.CreateVenture;
using FinanceDAMT.Application.Features.Ventures.Commands.DeleteBatch;
using FinanceDAMT.Application.Features.Ventures.Commands.DeleteVenture;
using FinanceDAMT.Application.Features.Ventures.Commands.UpdateBatch;
using FinanceDAMT.Application.Features.Ventures.Commands.UpdateVenture;
using FinanceDAMT.Application.Features.Ventures.DTOs;
using FinanceDAMT.Application.Features.Ventures.Queries.GetVentureById;
using FinanceDAMT.Application.Features.Ventures.Queries.GetVentures;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceDAMT.API.Controllers;

/// <summary>
/// Endpoints for entrepreneurship ventures and their production batches (ROI).
/// </summary>
[ApiController]
[Authorize]
[Route("api/ventures")]
[Produces("application/json")]
public class VenturesController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="VenturesController"/> class.
    /// </summary>
    /// <param name="mediator">MediatR request dispatcher.</param>
    public VenturesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Returns all ventures for the current user with rolled-up ROI metrics.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<VentureDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVentures(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetVenturesQuery(), ct);
        return Ok(result);
    }

    /// <summary>Returns a single venture including its production batches.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VentureDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVentureById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetVentureByIdQuery(id), ct);
        return Ok(result);
    }

    /// <summary>Creates a venture.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(VentureDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateVentureRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateVentureCommand(request.Name, request.Icon, request.Description), ct);
        return CreatedAtAction(nameof(GetVentureById), new { id = result.Id }, result);
    }

    /// <summary>Updates a venture.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(VentureDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVentureRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateVentureCommand(id, request.Name, request.Icon, request.Description, request.IsActive), ct);
        return Ok(result);
    }

    /// <summary>Soft deletes a venture and its batches.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteVentureCommand(id), ct);
        return NoContent();
    }

    /// <summary>Adds a production batch to a venture.</summary>
    [HttpPost("{id:guid}/batches")]
    [ProducesResponseType(typeof(VentureDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddBatch(Guid id, [FromBody] CreateBatchRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new AddBatchCommand(id, request.Label, request.Date, request.Investment, request.UnitsProduced, request.Income, request.Notes), ct);
        return Ok(result);
    }

    /// <summary>Updates a production batch.</summary>
    [HttpPut("{id:guid}/batches/{bid:guid}")]
    [ProducesResponseType(typeof(VentureDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateBatch(Guid id, Guid bid, [FromBody] UpdateBatchRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new UpdateBatchCommand(id, bid, request.Label, request.Date, request.Investment, request.UnitsProduced, request.Income, request.Notes), ct);
        return Ok(result);
    }

    /// <summary>Soft deletes a production batch.</summary>
    [HttpDelete("{id:guid}/batches/{bid:guid}")]
    [ProducesResponseType(typeof(VentureDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteBatch(Guid id, Guid bid, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteBatchCommand(id, bid), ct);
        return Ok(result);
    }
}
