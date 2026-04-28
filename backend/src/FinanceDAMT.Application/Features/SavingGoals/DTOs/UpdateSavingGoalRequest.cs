namespace FinanceDAMT.Application.Features.SavingGoals.DTOs;

public sealed record UpdateSavingGoalRequest(
    string Name,
    decimal TargetAmount,
    DateTime Deadline,
    string Icon
);
