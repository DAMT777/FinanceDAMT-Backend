using FinanceDAMT.Application.Features.Subscriptions.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.Subscriptions.Queries.GetSubscriptions;

public sealed record GetSubscriptionsQuery : IRequest<IReadOnlyList<SubscriptionDto>>;
