using MediatR;

namespace FinanceDAMT.Application.Features.Accounts.Commands.TransferBetweenAccounts;

public sealed record TransferBetweenAccountsCommand(
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount
) : IRequest<Unit>;
