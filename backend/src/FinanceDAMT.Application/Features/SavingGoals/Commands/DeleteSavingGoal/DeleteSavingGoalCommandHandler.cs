using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.SavingGoals.Commands.DeleteSavingGoal;

public sealed class DeleteSavingGoalCommandHandler : IRequestHandler<DeleteSavingGoalCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteSavingGoalCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteSavingGoalCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var goal = await _context.SavingGoals
            .FirstOrDefaultAsync(g => g.Id == request.Id && g.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Saving goal not found.");

        goal.IsDeleted = true;
        goal.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
