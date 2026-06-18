using FinanceDAMT.Application.Features.Subscriptions.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.Subscriptions.Queries.GetSubscriptionById;

public sealed record GetSubscriptionByIdQuery(Guid Id) : IRequest<SubscriptionDto>;
