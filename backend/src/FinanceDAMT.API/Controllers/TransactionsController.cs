using FinanceDAMT.Application.Features.Transactions.Commands.CreateRecurringTransaction;
using FinanceDAMT.Application.Features.Transactions.Commands.CreateTransaction;
using FinanceDAMT.Application.Features.Transactions.Commands.DeleteTransaction;
using FinanceDAMT.Application.Features.Transactions.Commands.UpdateTransaction;
using FinanceDAMT.Application.Features.Transactions.Commands.UploadTransactionReceipt;
using FinanceDAMT.Application.Features.Transactions.DTOs;
using FinanceDAMT.Application.Features.Transactions.Queries.GetRecurringTransactions;
using FinanceDAMT.Application.Features.Transactions.Queries.GetTransactionById;
using FinanceDAMT.Application.Features.Transactions.Queries.GetTransactions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceDAMT.API.Controllers;

/// <summary>
/// Endpoints for managing financial transactions.
/// </summary>
[ApiController]
[Authorize]
[Route("api/transactions")]
[Produces("application/json")]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionsController"/> class.
    /// </summary>
    /// <param name="mediator">MediatR request dispatcher.</param>
    public TransactionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Returns a paginated and filterable transaction list.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] Guid? accountId,
        [FromQuery] Guid? categoryId,
        [FromQuery] FinanceDAMT.Domain.Enums.TransactionType? type,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? searchText,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var filter = new TransactionFilterRequest(accountId, categoryId, type, dateFrom, dateTo, searchText, page, pageSize);
        var result = await _mediator.Send(new GetTransactionsQuery(filter), ct);
        return Ok(result);
    }

    /// <summary>
    /// Returns a single transaction by identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransactionById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTransactionByIdQuery(id), ct);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new income or expense transaction.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreateTransactionCommand(request.AccountId, request.CategoryId, request.Type, request.Amount, request.Date, request.Description),
            ct);

        return CreatedAtAction(nameof(GetTransactionById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing transaction.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTransactionRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new UpdateTransactionCommand(
                id,
                request.AccountId,
                request.CategoryId,
                request.Type,
                request.Amount,
                request.Date,
                request.Description,
                request.IsRecurring),
            ct);

        return Ok(result);
    }

    /// <summary>
    /// Soft deletes a transaction.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteTransactionCommand(id), ct);
        return NoContent();
    }

    /// <summary>
    /// Uploads a receipt image for a transaction.
    /// </summary>
    [HttpPost("{id:guid}/receipt")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadReceipt(Guid id, [FromForm] UploadReceiptRequest request, CancellationToken ct)
    {
        var file = request.File;
        if (file is null)
            return BadRequest(new { message = "File is required." });

        if (file.Length == 0)
            return BadRequest(new { message = "File is empty." });

        await using var stream = file.OpenReadStream();
        var url = await _mediator.Send(new UploadTransactionReceiptCommand(id, stream, file.FileName, file.ContentType), ct);

        return Ok(new { receiptUrl = url });
    }

    /// <summary>
    /// Returns recurring transactions for the current user.
    /// </summary>
    [HttpGet("recurring")]
    [ProducesResponseType(typeof(IReadOnlyList<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecurringTransactions(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetRecurringTransactionsQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Creates a recurring transaction entry.
    /// </summary>
    [HttpPost("recurring")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRecurring([FromBody] CreateTransactionRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreateRecurringTransactionCommand(request.AccountId, request.CategoryId, request.Type, request.Amount, request.Date, request.Description),
            ct);

        return CreatedAtAction(nameof(GetTransactionById), new { id = result.Id }, result);
    }
}

/// <summary>
/// Request payload for receipt upload.
/// </summary>
public sealed class UploadReceiptRequest
{
    /// <summary>
    /// The receipt image file.
    /// </summary>
    public IFormFile? File { get; set; }
}
