namespace FinanceDAMT.Application.Features.AI.DTOs;

public sealed record FinancialScoreDto(
    int Score,
    int SavingsRatioPoints,
    int BudgetCompliancePoints,
    int GoalProgressPoints,
    int ExpenseDiversificationPoints,
    string Explanation
);
