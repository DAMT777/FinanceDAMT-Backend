namespace FinanceDAMT.Application.Features.AI.DTOs;

public sealed record MonthlySummaryDto(
    decimal TotalIncome,
    decimal TotalExpenses,
    string TopCategory,
    decimal PreviousMonthDeltaPercentage,
    IReadOnlyList<string> Anomalies,
    string Narrative
);
