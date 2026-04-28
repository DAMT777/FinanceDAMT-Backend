using System.Text.Json;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Domain.Entities;
using FinanceDAMT.Domain.Enums;
using FinanceDAMT.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FinanceDAMT.Infrastructure.Jobs;

public sealed class FinanceRecurringJobs : IFinanceRecurringJobs
{
    private readonly ApplicationDbContext _context;
    private readonly IAIService _aiService;
    private readonly IEmailService _emailService;
    private readonly IBudgetAlertService _budgetAlertService;
    private readonly ILogger<FinanceRecurringJobs> _logger;
    private readonly int _inactiveDays;

    public FinanceRecurringJobs(
        ApplicationDbContext context,
        IAIService aiService,
        IEmailService emailService,
        IBudgetAlertService budgetAlertService,
        IConfiguration configuration,
        ILogger<FinanceRecurringJobs> logger)
    {
        _context = context;
        _aiService = aiService;
        _emailService = emailService;
        _budgetAlertService = budgetAlertService;
        _logger = logger;
        _inactiveDays = configuration.GetValue<int>("BackgroundJobs:InactivityDays", 7);
    }

    public async Task GenerateMonthlySummariesAsync(CancellationToken cancellationToken = default)
    {
        var target = DateTime.UtcNow.AddMonths(-1);
        var users = await _context.Users
            .AsNoTracking()
            .Where(u => !u.IsDeleted)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        foreach (var userId in users)
        {
            try
            {
                var summary = await _aiService.GenerateMonthlySummary(userId, target.Month, target.Year);

                _context.AIRecommendations.Add(new AIRecommendation
                {
                    UserId = userId,
                    Type = AIRecommendationType.Summary,
                    Content = summary.Narrative,
                    GeneratedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Monthly summary generation failed for user {UserId}", userId);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RunDailyBudgetChecksAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var month = now.Month;
        var year = now.Year;

        var budgets = await _context.Budgets
            .Include(b => b.Category)
            .AsTracking()
            .Where(b => b.Month == month && b.Year == year)
            .ToListAsync(cancellationToken);

        foreach (var budget in budgets)
        {
            var was80 = budget.AlertSent80;
            var was100 = budget.AlertSent100;

            await _budgetAlertService.CheckThresholdsAsync(budget.UserId, budget.CategoryId, month, year, cancellationToken);

            if (!was80 && budget.AlertSent80)
            {
                _context.AIRecommendations.Add(new AIRecommendation
                {
                    UserId = budget.UserId,
                    Type = AIRecommendationType.Alert,
                    Content = $"BUDGET_80:{budget.Category.Name}:You have reached 80% of your budget for {budget.Category.Name}.",
                    GeneratedAt = DateTime.UtcNow
                });
            }

            if (!was100 && budget.AlertSent100)
            {
                _context.AIRecommendations.Add(new AIRecommendation
                {
                    UserId = budget.UserId,
                    Type = AIRecommendationType.Alert,
                    Content = $"BUDGET_100:{budget.Category.Name}:You have exceeded your budget for {budget.Category.Name}.",
                    GeneratedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SendInactiveUserRemindersAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var cutoff = now.AddDays(-_inactiveDays);
        var reminderWindow = now.AddDays(-3);

        var users = await _context.Users
            .AsNoTracking()
            .Where(u => !u.IsDeleted)
            .Select(u => new { u.Id, u.Email, u.Name })
            .ToListAsync(cancellationToken);

        foreach (var user in users)
        {
            var lastTransactionDate = await _context.Transactions
                .Where(t => t.UserId == user.Id)
                .MaxAsync(t => (DateTime?)t.Date, cancellationToken);

            var isInactive = !lastTransactionDate.HasValue || lastTransactionDate.Value < cutoff;
            if (!isInactive)
                continue;

            var alreadyReminded = await _context.AIRecommendations
                .AsNoTracking()
                .AnyAsync(
                    r => r.UserId == user.Id &&
                         r.Type == AIRecommendationType.Alert &&
                         r.GeneratedAt >= reminderWindow &&
                         r.Content.StartsWith("INACTIVITY_REMINDER:"),
                    cancellationToken);

            if (alreadyReminded)
                continue;

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                await _emailService.SendEmailAsync(
                    user.Email,
                    user.Name,
                    "We miss you in FinanceDAMT",
                    "<p>It has been a while since your last transaction. Track your expenses and keep your goals on track.</p>",
                    cancellationToken);
            }

            _context.AIRecommendations.Add(new AIRecommendation
            {
                UserId = user.Id,
                Type = AIRecommendationType.Alert,
                Content = "INACTIVITY_REMINDER:User has no recent activity.",
                GeneratedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CalculateMonthlyScoresAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var users = await _context.Users
            .AsNoTracking()
            .Where(u => !u.IsDeleted)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        foreach (var userId in users)
        {
            try
            {
                var score = await _aiService.CalculateFinancialScore(userId);

                var existing = await _context.FinancialScores
                    .FirstOrDefaultAsync(
                        f => f.UserId == userId && f.Month == now.Month && f.Year == now.Year,
                        cancellationToken);

                if (existing is null)
                {
                    _context.FinancialScores.Add(new FinancialScore
                    {
                        UserId = userId,
                        Score = score.Score,
                        Month = now.Month,
                        Year = now.Year,
                        Breakdown = JsonSerializer.Serialize(score)
                    });
                }
                else
                {
                    existing.Score = score.Score;
                    existing.Breakdown = JsonSerializer.Serialize(score);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Monthly score calculation failed for user {UserId}", userId);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
