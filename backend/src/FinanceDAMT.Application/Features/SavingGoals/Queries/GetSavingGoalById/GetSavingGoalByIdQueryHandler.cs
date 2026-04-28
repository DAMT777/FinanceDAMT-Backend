using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.SavingGoals.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.SavingGoals.Queries.GetSavingGoalById;

public sealed class GetSavingGoalByIdQueryHandler : IRequestHandler<GetSavingGoalByIdQuery, SavingGoalDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetSavingGoalByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SavingGoalDto> Handle(GetSavingGoalByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var goal = await _context.SavingGoals
            .AsNoTracking()
            .Include(g => g.Contributions)
            .FirstOrDefaultAsync(g => g.Id == request.Id && g.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Saving goal not found.");

        return SavingGoalProjection.ToDto(goal);
    }
}
