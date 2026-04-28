using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.SavingGoals.DTOs;
using FinanceDAMT.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.SavingGoals.Commands.AddContribution;

public sealed class AddContributionCommandHandler : IRequestHandler<AddContributionCommand, SavingGoalDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AddContributionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
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

        goal.CurrentAmount = goal.Contributions.Sum(c => c.Amount);
        goal.IsCompleted = goal.CurrentAmount >= goal.TargetAmount;
        goal.MilestonesReached = SavingGoalProjection.CalculateMilestones(goal.CurrentAmount, goal.TargetAmount);

        await _context.SaveChangesAsync(cancellationToken);
        return SavingGoalProjection.ToDto(goal);
    }
}
