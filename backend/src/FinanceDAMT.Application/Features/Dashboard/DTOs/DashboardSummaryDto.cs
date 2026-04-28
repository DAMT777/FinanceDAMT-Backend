namespace FinanceDAMT.Application.Features.Dashboard.DTOs;

public sealed record DashboardSummaryDto(
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal CurrentBalance,
    IReadOnlyList<CategoryExpenseDto> ExpenseBreakdown,
    IReadOnlyList<MonthlyIncomeExpenseDto> IncomeVsExpensesLast6Months,
    IReadOnlyList<MonthlyBalancePointDto> BalanceEvolutionLast6Months,
    IReadOnlyList<DailySpendingPointDto> SpendingHeatmap,
    MonthOverMonthComparisonDto MonthOverMonthComparison,
    decimal EndOfMonthProjection
);

public sealed record CategoryExpenseDto(string CategoryName, decimal Amount);

public sealed record MonthlyIncomeExpenseDto(int Year, int Month, decimal Income, decimal Expenses);

public sealed record MonthlyBalancePointDto(int Year, int Month, decimal Balance);

public sealed record DailySpendingPointDto(int Day, decimal Amount);

public sealed record MonthOverMonthComparisonDto(decimal CurrentMonthExpenses, decimal PreviousMonthExpenses, decimal ChangePercentage);
