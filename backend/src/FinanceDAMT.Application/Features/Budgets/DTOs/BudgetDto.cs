namespace FinanceDAMT.Application.Features.Budgets.DTOs;

public sealed record BudgetDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    decimal MonthlyLimit,
    int Month,
    int Year,
    bool AlertSent80,
    bool AlertSent100
);
