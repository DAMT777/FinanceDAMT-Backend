using FinanceDAMT.Domain.Enums;

namespace FinanceDAMT.Application.Features.Accounts.DTOs;

public sealed record UpdateAccountRequest(
    string Name,
    AccountType Type,
    decimal Balance,
    decimal? CreditLimit,
    int? CutoffDay,
    int? PaymentDay
);
