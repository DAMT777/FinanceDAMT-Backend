using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.SavingGoals.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.SavingGoals.Commands.UpdateSavingGoal;

public sealed class UpdateSavingGoalCommandHandler : IRequestHandler<UpdateSavingGoalCommand, SavingGoalDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateSavingGoalCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SavingGoalDto> Handle(UpdateSavingGoalCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var goal = await _context.SavingGoals
            .Include(g => g.Contributions)
            .FirstOrDefaultAsync(g => g.Id == request.Id && g.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Saving goal not found.");

        goal.Name = request.Name.Trim();
        goal.TargetAmount = request.TargetAmount;
        goal.Deadline = request.Deadline;
        goal.Icon = request.Icon.Trim();
        goal.IsCompleted = goal.CurrentAmount >= goal.TargetAmount;
        goal.MilestonesReached = SavingGoalProjection.CalculateMilestones(goal.CurrentAmount, goal.TargetAmount);

        await _context.SaveChangesAsync(cancellationToken);
        return SavingGoalProjection.ToDto(goal);
    }
}
