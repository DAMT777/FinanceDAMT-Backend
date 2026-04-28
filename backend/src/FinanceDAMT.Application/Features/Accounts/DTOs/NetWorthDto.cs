namespace FinanceDAMT.Application.Features.Accounts.DTOs;

public sealed record NetWorthDto(
    decimal Assets,
    decimal Liabilities,
    decimal NetWorth
);
