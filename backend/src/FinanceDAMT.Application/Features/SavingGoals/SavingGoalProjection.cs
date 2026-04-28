using FinanceDAMT.Application.Features.SavingGoals.DTOs;
using FinanceDAMT.Domain.Entities;

namespace FinanceDAMT.Application.Features.SavingGoals;

internal static class SavingGoalProjection
{
    public static int CalculateMilestones(decimal currentAmount, decimal targetAmount)
    {
        if (targetAmount <= 0) return 0;

        var ratio = currentAmount / targetAmount;
        var mask = 0;

        if (ratio >= 0.25m) mask |= 1;
        if (ratio >= 0.50m) mask |= 2;
        if (ratio >= 0.75m) mask |= 4;
        if (ratio >= 1.00m) mask |= 8;

        return mask;
    }

    public static DateTime? EstimateCompletionDate(SavingGoal goal)
    {
        if (goal.CurrentAmount >= goal.TargetAmount)
            return DateTime.UtcNow.Date;

        var fromDate = DateTime.UtcNow.Date.AddMonths(-3);
        var recentContributions = goal.Contributions
            .Where(c => c.Date >= fromDate)
            .OrderBy(c => c.Date)
            .ToList();

        var totalRecent = recentContributions.Sum(c => c.Amount);
        var averageMonthly = totalRecent / 3m;

        if (averageMonthly <= 0)
            return null;

        var remaining = goal.TargetAmount - goal.CurrentAmount;
        var monthsNeeded = remaining / averageMonthly;
        var daysNeeded = (int)Math.Ceiling(monthsNeeded * 30m);

        return DateTime.UtcNow.Date.AddDays(daysNeeded);
    }

    public static SavingGoalDto ToDto(SavingGoal goal)
    {
        var contributions = goal.Contributions
            .OrderByDescending(c => c.Date)
            .Select(c => new SavingContributionDto(c.Id, c.Amount, c.Date, c.Note))
            .ToList();

        return new SavingGoalDto(
            goal.Id,
            goal.Name,
            goal.TargetAmount,
            goal.CurrentAmount,
            goal.Deadline,
            goal.Icon,
            goal.IsCompleted,
            goal.MilestonesReached,
            EstimateCompletionDate(goal),
            contributions);
    }
}
