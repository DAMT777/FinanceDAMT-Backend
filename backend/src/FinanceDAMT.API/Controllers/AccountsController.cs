using FinanceDAMT.Application.Features.Accounts.Commands.CreateAccount;
using FinanceDAMT.Application.Features.Accounts.Commands.DeleteAccount;
using FinanceDAMT.Application.Features.Accounts.Commands.TransferBetweenAccounts;
using FinanceDAMT.Application.Features.Accounts.Commands.UpdateAccount;
using FinanceDAMT.Application.Features.Accounts.DTOs;
using FinanceDAMT.Application.Features.Accounts.Queries.GetAccountById;
using FinanceDAMT.Application.Features.Accounts.Queries.GetAccounts;
using FinanceDAMT.Application.Features.Accounts.Queries.GetNetWorth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceDAMT.API.Controllers;

/// <summary>
/// Endpoints for managing user financial accounts.
/// </summary>
[ApiController]
[Authorize]
[Route("api/accounts")]
[Produces("application/json")]
public class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountsController"/> class.
    /// </summary>
    /// <param name="mediator">MediatR request dispatcher.</param>
    public AccountsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Returns all accounts for the current user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AccountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccounts(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAccountsQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Returns a single account by identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccountById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAccountByIdQuery(id), ct);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new account for the current user.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAccountRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreateAccountCommand(
                request.Name,
                request.Type,
                request.Balance,
                request.CreditLimit,
                request.CutoffDay,
                request.PaymentDay),
            ct);

        return CreatedAtAction(nameof(GetAccountById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing account.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAccountRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new UpdateAccountCommand(
                id,
                request.Name,
                request.Type,
                request.Balance,
                request.CreditLimit,
                request.CutoffDay,
                request.PaymentDay),
            ct);

        return Ok(result);
    }

    /// <summary>
    /// Soft deletes an account.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteAccountCommand(id), ct);
        return NoContent();
    }

    /// <summary>
    /// Transfers funds between two user accounts.
    /// </summary>
    [HttpPost("transfer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Transfer([FromBody] TransferBetweenAccountsRequest request, CancellationToken ct)
    {
        await _mediator.Send(new TransferBetweenAccountsCommand(request.FromAccountId, request.ToAccountId, request.Amount), ct);
        return Ok(new { message = "Transfer completed successfully." });
    }

    /// <summary>
    /// Returns current user net worth as assets minus liabilities.
    /// </summary>
    [HttpGet("net-worth")]
    [ProducesResponseType(typeof(NetWorthDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNetWorth(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetNetWorthQuery(), ct);
        return Ok(result);
    }
}
