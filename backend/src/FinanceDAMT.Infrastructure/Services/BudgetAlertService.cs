using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Domain.Enums;
using FinanceDAMT.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Infrastructure.Services;

public sealed class BudgetAlertService : IBudgetAlertService
{
    private readonly ApplicationDbContext _context;

    public BudgetAlertService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CheckThresholdsAsync(Guid userId, Guid categoryId, int month, int year, CancellationToken cancellationToken = default)
    {
        var budget = await _context.Budgets
            .FirstOrDefaultAsync(
                b => b.UserId == userId && b.CategoryId == categoryId && b.Month == month && b.Year == year,
                cancellationToken);

        if (budget is null || budget.MonthlyLimit <= 0)
            return;

        var monthStart = new DateTime(year, month, 1);
        var monthEnd = monthStart.AddMonths(1);

        var spent = await _context.Transactions
            .Where(t =>
                t.UserId == userId &&
                t.CategoryId == categoryId &&
                t.Type == TransactionType.Expense &&
                t.Date >= monthStart &&
                t.Date < monthEnd)
            .SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0m;

        var ratio = spent / budget.MonthlyLimit;
        var changed = false;

        if (ratio >= 0.80m && !budget.AlertSent80)
        {
            budget.AlertSent80 = true;
            changed = true;
        }

        if (ratio >= 1.00m && !budget.AlertSent100)
        {
            budget.AlertSent100 = true;
            changed = true;
        }

        if (changed)
            await _context.SaveChangesAsync(cancellationToken);
    }
}
