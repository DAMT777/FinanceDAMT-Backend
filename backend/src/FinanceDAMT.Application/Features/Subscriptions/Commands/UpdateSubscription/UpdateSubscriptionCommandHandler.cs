using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.Subscriptions.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.Subscriptions.Commands.UpdateSubscription;

public sealed class UpdateSubscriptionCommandHandler : IRequestHandler<UpdateSubscriptionCommand, SubscriptionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateSubscriptionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SubscriptionDto> Handle(UpdateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == request.Id && s.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Subscription not found.");

        subscription.Name = request.Name.Trim();
        subscription.Amount = request.Amount;
        subscription.BillingCycle = request.BillingCycle;
        subscription.NextBillingDate = request.NextBillingDate;
        subscription.Icon = request.Icon.Trim();
        subscription.IsActive = request.IsActive;
        subscription.Notes = request.Notes?.Trim();

        await _context.SaveChangesAsync(cancellationToken);
        return SubscriptionProjection.ToDto(subscription);
    }
}
