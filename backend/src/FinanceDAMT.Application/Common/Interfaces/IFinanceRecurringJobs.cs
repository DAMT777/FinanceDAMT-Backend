namespace FinanceDAMT.Application.Common.Interfaces;

public interface IFinanceRecurringJobs
{
    Task GenerateMonthlySummariesAsync(CancellationToken cancellationToken = default);
    Task RunDailyBudgetChecksAsync(CancellationToken cancellationToken = default);
    Task SendInactiveUserRemindersAsync(CancellationToken cancellationToken = default);
    Task CalculateMonthlyScoresAsync(CancellationToken cancellationToken = default);
}
