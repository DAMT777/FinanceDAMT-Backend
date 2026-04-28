namespace FinanceDAMT.Application.Features.Transactions.DTOs;

public sealed record PaginatedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount
);
