using MediatR;

namespace FinanceDAMT.Application.Features.Subscriptions.Commands.DeleteSubscription;

public sealed record DeleteSubscriptionCommand(Guid Id) : IRequest<Unit>;
