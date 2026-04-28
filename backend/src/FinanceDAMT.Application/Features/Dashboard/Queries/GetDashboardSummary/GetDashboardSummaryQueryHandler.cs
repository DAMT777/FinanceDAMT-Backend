using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.Dashboard.DTOs;
using FinanceDAMT.Domain.Enums;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.Dashboard.Queries.GetDashboardSummary;

public sealed class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public GetDashboardSummaryQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser, ICacheService cache)
    {
        _context = context;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");
        var cacheKey = $"dashboard:{userId}:{request.Year}:{request.Month}";

        if (request.Month is < 1 or > 12)
            throw new ValidationException(
                new[] { new ValidationFailure(nameof(request.Month), $"Invalid month value '{request.Month}'. Expected 1-12.") });

        if (request.Year is < 1900 or > 3000)
            throw new ValidationException(
                new[] { new ValidationFailure(nameof(request.Year), $"Invalid year value '{request.Year}'.") });

        var cached = await _cache.GetAsync<DashboardSummaryDto>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var monthStart = new DateTime(request.Year, request.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);
        var previousMonthStart = monthStart.AddMonths(-1);
        var previousMonthEnd = monthStart;
        var sixMonthsStart = monthStart.AddMonths(-5);

        var currentMonthTransactions = _context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.Date >= monthStart && t.Date < monthEnd);

        var totalIncome = await currentMonthTransactions
            .Where(t => t.Type == TransactionType.Income)
            .SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0m;

        var totalExpenses = await currentMonthTransactions
            .Where(t => t.Type == TransactionType.Expense)
            .SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0m;

        var currentBalance = await _context.Accounts
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .SumAsync(a => (decimal?)a.Balance, cancellationToken) ?? 0m;

        var expenseRows = await currentMonthTransactions
            .Where(t => t.Type == TransactionType.Expense)
            .Select(t => new
            {
                CategoryName = t.Category != null ? t.Category.Name : null,
                t.Amount
            })
            .ToListAsync(cancellationToken);

        var expenseBreakdown = expenseRows
            .GroupBy(t => string.IsNullOrWhiteSpace(t.CategoryName) ? "Uncategorized" : t.CategoryName!)
            .Select(g => new CategoryExpenseDto(g.Key, g.Sum(x => x.Amount)))
            .OrderByDescending(x => x.Amount)
            .ToList();

        var monthlyAggregatesRaw = await _context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.Date >= sixMonthsStart && t.Date < monthEnd)
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Income = g.Where(x => x.Type == TransactionType.Income).Sum(x => (decimal?)x.Amount) ?? 0m,
                Expenses = g.Where(x => x.Type == TransactionType.Expense).Sum(x => (decimal?)x.Amount) ?? 0m
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToListAsync(cancellationToken);

        var monthlyAggregates = monthlyAggregatesRaw
            .Select(x => new MonthlyIncomeExpenseDto(x.Year, x.Month, x.Income, x.Expenses))
            .ToList();

        var balanceEvolution = new List<MonthlyBalancePointDto>();
        decimal runningBalance = 0m;
        foreach (var item in monthlyAggregates)
        {
            runningBalance += item.Income - item.Expenses;
            balanceEvolution.Add(new MonthlyBalancePointDto(item.Year, item.Month, runningBalance));
        }

        // Ensure fixed 6-month shape for new users so clients can safely render charts.
        if (monthlyAggregates.Count == 0)
        {
            for (var i = 5; i >= 0; i--)
            {
                var monthPoint = monthStart.AddMonths(-i);
                balanceEvolution.Add(new MonthlyBalancePointDto(monthPoint.Year, monthPoint.Month, 0m));
            }
        }

        var spendingHeatmapRaw = await currentMonthTransactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.Date.Day)
            .Select(g => new
            {
                Day = g.Key,
                Amount = g.Sum(x => (decimal?)x.Amount) ?? 0m
            })
            .OrderBy(x => x.Day)
            .ToListAsync(cancellationToken);

        var spendingHeatmap = spendingHeatmapRaw
            .Select(x => new DailySpendingPointDto(x.Day, x.Amount))
            .ToList();

        var previousMonthExpenses = await _context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.Type == TransactionType.Expense && t.Date >= previousMonthStart && t.Date < previousMonthEnd)
            .SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0m;

        var changePercentage = previousMonthExpenses == 0m
            ? (totalExpenses > 0m ? 100m : 0m)
            : Math.Round(((totalExpenses - previousMonthExpenses) / previousMonthExpenses) * 100m, 2);

        var now = DateTime.UtcNow;
        var dayForProjection = (request.Month == now.Month && request.Year == now.Year) ? now.Day : DateTime.DaysInMonth(request.Year, request.Month);
        var daysInMonth = DateTime.DaysInMonth(request.Year, request.Month);
        var averageDailySpent = dayForProjection == 0 ? 0m : totalExpenses / dayForProjection;
        var projection = Math.Round(averageDailySpent * daysInMonth, 2);

        var summary = new DashboardSummaryDto(
            totalIncome,
            totalExpenses,
            currentBalance,
            expenseBreakdown.Count == 0 ? Array.Empty<CategoryExpenseDto>() : expenseBreakdown,
            monthlyAggregates.Count == 0 ? BuildEmptyMonthlyAggregates(monthStart) : monthlyAggregates,
            balanceEvolution,
            spendingHeatmap.Count == 0 ? Array.Empty<DailySpendingPointDto>() : spendingHeatmap,
            new MonthOverMonthComparisonDto(totalExpenses, previousMonthExpenses, changePercentage),
            projection);

        await _cache.SetAsync(cacheKey, summary, TimeSpan.FromMinutes(15), cancellationToken);
        return summary;
    }

    private static IReadOnlyList<MonthlyIncomeExpenseDto> BuildEmptyMonthlyAggregates(DateTime monthStart)
    {
        var result = new List<MonthlyIncomeExpenseDto>(capacity: 6);
        for (var i = 5; i >= 0; i--)
        {
            var p = monthStart.AddMonths(-i);
            result.Add(new MonthlyIncomeExpenseDto(p.Year, p.Month, 0m, 0m));
        }

        return result;
    }
}
