using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.Subscriptions.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.Subscriptions.Queries.GetSubscriptionById;

public sealed class GetSubscriptionByIdQueryHandler : IRequestHandler<GetSubscriptionByIdQuery, SubscriptionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetSubscriptionByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SubscriptionDto> Handle(GetSubscriptionByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var subscription = await _context.Subscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.Id && s.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Subscription not found.");

        return SubscriptionProjection.ToDto(subscription);
    }
}
