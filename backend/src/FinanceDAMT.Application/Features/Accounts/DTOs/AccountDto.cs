using FinanceDAMT.Domain.Enums;

namespace FinanceDAMT.Application.Features.Accounts.DTOs;

public sealed record AccountDto(
    Guid Id,
    string Name,
    AccountType Type,
    decimal Balance,
    decimal? CreditLimit,
    int? CutoffDay,
    int? PaymentDay
);
