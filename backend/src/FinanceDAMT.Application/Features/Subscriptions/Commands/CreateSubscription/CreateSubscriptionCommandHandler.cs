using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.Subscriptions.DTOs;
using FinanceDAMT.Domain.Entities;
using MediatR;

namespace FinanceDAMT.Application.Features.Subscriptions.Commands.CreateSubscription;

public sealed class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, SubscriptionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateSubscriptionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SubscriptionDto> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var subscription = new Subscription
        {
            UserId = userId,
            Name = request.Name.Trim(),
            Amount = request.Amount,
            BillingCycle = request.BillingCycle,
            NextBillingDate = request.NextBillingDate,
            Icon = request.Icon.Trim(),
            Notes = request.Notes?.Trim(),
            IsActive = true
        };

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        return SubscriptionProjection.ToDto(subscription);
    }
}
