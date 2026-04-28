namespace FinanceDAMT.Application.Features.SavingGoals.DTOs;

public sealed record AddContributionRequest(
    decimal Amount,
    DateTime Date,
    string? Note
);
