using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.SavingGoals.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.SavingGoals.Queries.GetSavingGoals;

public sealed class GetSavingGoalsQueryHandler : IRequestHandler<GetSavingGoalsQuery, IReadOnlyList<SavingGoalDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetSavingGoalsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<SavingGoalDto>> Handle(GetSavingGoalsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var goals = await _context.SavingGoals
            .AsNoTracking()
            .Include(g => g.Contributions)
            .Where(g => g.UserId == userId)
            .OrderBy(g => g.Deadline)
            .ToListAsync(cancellationToken);

        return goals.Select(SavingGoalProjection.ToDto).ToList();
    }
}
