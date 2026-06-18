using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.Subscriptions.Commands.DeleteSubscription;

public sealed class DeleteSubscriptionCommandHandler : IRequestHandler<DeleteSubscriptionCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteSubscriptionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == request.Id && s.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Subscription not found.");

        subscription.IsDeleted = true;
        subscription.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
