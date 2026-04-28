using FinanceDAMT.Domain.Enums;

namespace FinanceDAMT.Application.Features.Transactions.DTOs;

public sealed record CreateTransactionRequest(
    Guid AccountId,
    Guid CategoryId,
    TransactionType Type,
    decimal Amount,
    DateTime Date,
    string? Description
);
