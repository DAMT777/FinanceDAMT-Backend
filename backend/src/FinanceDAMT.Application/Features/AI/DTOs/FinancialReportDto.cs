using FinanceDAMT.Domain.Enums;

namespace FinanceDAMT.Application.Features.AI.DTOs;

public sealed record ReportCategoryDto(string CategoryName, decimal Amount, double Percentage);

public sealed record FinancialReportDto(
    ReportPeriod Period,
    string PeriodLabel,
    DateTime StartDate,
    DateTime EndDate,
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal Net,
    int TransactionCount,
    IReadOnlyList<ReportCategoryDto> TopExpenseCategories,
    string Message
);
