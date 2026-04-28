using FinanceDAMT.Domain.Enums;

namespace FinanceDAMT.Application.Features.Transactions.DTOs;

public sealed record TransactionDto(
    Guid Id,
    Guid AccountId,
    string AccountName,
    Guid CategoryId,
    string CategoryName,
    TransactionType Type,
    decimal Amount,
    DateTime Date,
    string? Description,
    string? ReceiptUrl,
    bool IsRecurring
);
