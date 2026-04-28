using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.SavingGoals.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.SavingGoals.Commands.RemoveContribution;

public sealed class RemoveContributionCommandHandler : IRequestHandler<RemoveContributionCommand, SavingGoalDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public RemoveContributionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SavingGoalDto> Handle(RemoveContributionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var goal = await _context.SavingGoals
            .Include(g => g.Contributions)
            .FirstOrDefaultAsync(g => g.Id == request.GoalId && g.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Saving goal not found.");

        var contribution = goal.Contributions.FirstOrDefault(c => c.Id == request.ContributionId)
            ?? throw new NotFoundException("Contribution not found.");

        contribution.IsDeleted = true;
        contribution.DeletedAt = DateTime.UtcNow;

        goal.CurrentAmount = goal.Contributions.Where(c => !c.IsDeleted).Sum(c => c.Amount);
        goal.IsCompleted = goal.CurrentAmount >= goal.TargetAmount;
        goal.MilestonesReached = SavingGoalProjection.CalculateMilestones(goal.CurrentAmount, goal.TargetAmount);

        await _context.SaveChangesAsync(cancellationToken);

        var activeContributions = goal.Contributions.Where(c => !c.IsDeleted).ToList();
        goal.Contributions = activeContributions;
        return SavingGoalProjection.ToDto(goal);
    }
}
