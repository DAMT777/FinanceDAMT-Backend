using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.AI.Queries.GetRecommendations;

public sealed class GetRecommendationsQueryHandler : IRequestHandler<GetRecommendationsQuery, IReadOnlyList<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetRecommendationsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<string>> Handle(GetRecommendationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        return await _context.AIRecommendations
            .AsNoTracking()
            .Where(r => r.UserId == userId && r.Type == AIRecommendationType.Proactive)
            .OrderByDescending(r => r.GeneratedAt)
            .Take(5)
            .Select(r => r.Content)
            .ToListAsync(cancellationToken);
    }
}
