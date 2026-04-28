using FinanceDAMT.Domain.Enums;

namespace FinanceDAMT.Application.Features.Transactions.DTOs;

public sealed record TransactionFilterRequest(
    Guid? AccountId,
    Guid? CategoryId,
    TransactionType? Type,
    DateTime? DateFrom,
    DateTime? DateTo,
    string? SearchText,
    int Page = 1,
    int PageSize = 20
);
