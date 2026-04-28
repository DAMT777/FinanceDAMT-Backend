namespace FinanceDAMT.Application.Features.SavingGoals.DTOs;

public sealed record CreateSavingGoalRequest(
    string Name,
    decimal TargetAmount,
    DateTime Deadline,
    string Icon
);
