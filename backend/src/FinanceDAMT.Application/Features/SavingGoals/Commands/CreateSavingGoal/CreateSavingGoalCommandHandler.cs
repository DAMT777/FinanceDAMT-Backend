using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.SavingGoals.DTOs;
using FinanceDAMT.Domain.Entities;
using MediatR;

namespace FinanceDAMT.Application.Features.SavingGoals.Commands.CreateSavingGoal;

public sealed class CreateSavingGoalCommandHandler : IRequestHandler<CreateSavingGoalCommand, SavingGoalDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateSavingGoalCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SavingGoalDto> Handle(CreateSavingGoalCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var goal = new SavingGoal
        {
            UserId = userId,
            Name = request.Name.Trim(),
            TargetAmount = request.TargetAmount,
            CurrentAmount = 0m,
            Deadline = request.Deadline,
            Icon = request.Icon.Trim(),
            MilestonesReached = 0,
            IsCompleted = false
        };

        _context.SavingGoals.Add(goal);
        await _context.SaveChangesAsync(cancellationToken);

        return SavingGoalProjection.ToDto(goal);
    }
}
