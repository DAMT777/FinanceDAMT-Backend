using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.Subscriptions.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.Subscriptions.Queries.GetSubscriptions;

public sealed class GetSubscriptionsQueryHandler : IRequestHandler<GetSubscriptionsQuery, IReadOnlyList<SubscriptionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetSubscriptionsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<SubscriptionDto>> Handle(GetSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var subscriptions = await _context.Subscriptions
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .OrderBy(s => s.NextBillingDate)
            .ToListAsync(cancellationToken);

        return subscriptions.Select(SubscriptionProjection.ToDto).ToList();
    }
}
