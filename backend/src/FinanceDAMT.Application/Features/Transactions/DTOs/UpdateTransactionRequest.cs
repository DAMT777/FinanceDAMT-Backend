using FinanceDAMT.Domain.Enums;

namespace FinanceDAMT.Application.Features.Transactions.DTOs;

public sealed record UpdateTransactionRequest(
    Guid AccountId,
    Guid CategoryId,
    TransactionType Type,
    decimal Amount,
    DateTime Date,
    string? Description,
    bool IsRecurring
);
