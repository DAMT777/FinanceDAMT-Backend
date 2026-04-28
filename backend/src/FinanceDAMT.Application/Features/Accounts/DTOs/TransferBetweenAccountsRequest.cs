namespace FinanceDAMT.Application.Features.Accounts.DTOs;

public sealed record TransferBetweenAccountsRequest(
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount
);
