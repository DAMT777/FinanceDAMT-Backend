namespace FinanceDAMT.Application.Features.SavingGoals.DTOs;

public sealed record SavingGoalDto(
    Guid Id,
    string Name,
    decimal TargetAmount,
    decimal CurrentAmount,
    DateTime Deadline,
    string Icon,
    bool IsCompleted,
    int MilestonesReached,
    DateTime? EstimatedCompletionDate,
    IReadOnlyList<SavingContributionDto> Contributions
);

public sealed record SavingContributionDto(
    Guid Id,
    decimal Amount,
    DateTime Date,
    string? Note
);
