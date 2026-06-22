using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.SavingGoals.DTOs;
using FinanceDAMT.Domain.Entities;
using FinanceDAMT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.SavingGoals.Commands.AddContribution;

public sealed class AddContributionCommandHandler : IRequestHandler<AddContributionCommand, SavingGoalDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;

    public AddContributionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        INotificationService notifications)
    {
        _context = context;
        _currentUser = currentUser;
        _notifications = notifications;
    }

    public async Task<SavingGoalDto> Handle(AddContributionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var goal = await _context.SavingGoals
            .Include(g => g.Contributions)
            .FirstOrDefaultAsync(g => g.Id == request.GoalId && g.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Saving goal not found.");

        goal.Contributions.Add(new SavingContribution
        {
            GoalId = goal.Id,
            Amount = request.Amount,
            Date = request.Date,
            Note = request.Note?.Trim()
        });

        var previousMilestones = goal.MilestonesReached;

        goal.CurrentAmount = goal.Contributions.Sum(c => c.Amount);
        goal.IsCompleted = goal.CurrentAmount >= goal.TargetAmount;
        goal.MilestonesReached = SavingGoalProjection.CalculateMilestones(goal.CurrentAmount, goal.TargetAmount);

        await _context.SaveChangesAsync(cancellationToken);

        // Notify when a new progress milestone (25/50/75/100%) is reached.
        var newMilestones = goal.MilestonesReached & ~previousMilestones;
        var milestonePercent = HighestMilestonePercent(newMilestones);
        if (milestonePercent > 0)
        {
            var title = milestonePercent >= 100 ? "🎉 ¡Meta completada!" : "¡Hito de meta alcanzado!";
            await _notifications.CreateAsync(
                userId,
                NotificationType.GoalMilestone,
                title,
                $"Llegaste al {milestonePercent}% de tu meta \"{goal.Name}\".",
                cancellationToken);
        }

        return SavingGoalProjection.ToDto(goal);
    }

    private static int HighestMilestonePercent(int milestoneMask)
    {
        if ((milestoneMask & 8) != 0) return 100;
        if ((milestoneMask & 4) != 0) return 75;
        if ((milestoneMask & 2) != 0) return 50;
        if ((milestoneMask & 1) != 0) return 25;
        return 0;
    }
}
