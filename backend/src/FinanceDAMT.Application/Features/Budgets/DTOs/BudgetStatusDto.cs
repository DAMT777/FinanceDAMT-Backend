namespace FinanceDAMT.Application.Features.Budgets.DTOs;

public sealed record BudgetStatusDto(
    Guid BudgetId,
    Guid CategoryId,
    string CategoryName,
    decimal MonthlyLimit,
    decimal Spent,
    decimal Percentage,
    bool AlertSent80,
    bool AlertSent100
);
