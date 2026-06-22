using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Domain.Enums;
using FinanceDAMT.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Infrastructure.Services;

public sealed class BudgetAlertService : IBudgetAlertService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notifications;

    public BudgetAlertService(ApplicationDbContext context, INotificationService notifications)
    {
        _context = context;
        _notifications = notifications;
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
        var percent = (int)Math.Round(ratio * 100);

        if (ratio >= 1.00m && !budget.AlertSent100)
        {
            budget.AlertSent80 = true;
            budget.AlertSent100 = true;
            var categoryName = await GetCategoryNameAsync(categoryId, cancellationToken);
            await _notifications.CreateAsync(
                userId,
                NotificationType.BudgetAlert,
                $"Presupuesto superado — {categoryName}",
                $"Superaste tu presupuesto de {categoryName} este mes ({percent}%).",
                cancellationToken);
        }
        else if (ratio >= 0.80m && !budget.AlertSent80)
        {
            budget.AlertSent80 = true;
            var categoryName = await GetCategoryNameAsync(categoryId, cancellationToken);
            await _notifications.CreateAsync(
                userId,
                NotificationType.BudgetAlert,
                $"Alerta de presupuesto — {categoryName}",
                $"Has usado el {percent}% de tu presupuesto de {categoryName} este mes.",
                cancellationToken);
        }
    }

    private async Task<string> GetCategoryNameAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        return await _context.Categories
            .Where(c => c.Id == categoryId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync(cancellationToken) ?? "tu categoría";
    }
}
