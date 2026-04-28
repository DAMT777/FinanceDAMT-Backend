namespace FinanceDAMT.Application.Features.Budgets.DTOs;

public sealed record SetBudgetRequest(
    Guid CategoryId,
    decimal MonthlyLimit,
    int Month,
    int Year
);
