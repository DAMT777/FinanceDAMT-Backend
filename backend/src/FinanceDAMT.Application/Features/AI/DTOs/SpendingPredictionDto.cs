namespace FinanceDAMT.Application.Features.AI.DTOs;

public sealed record SpendingPredictionDto(
    decimal TotalPredictedSpending,
    IReadOnlyList<CategoryPredictionDto> Categories,
    string Narrative
);

public sealed record CategoryPredictionDto(string CategoryName, decimal Amount);
