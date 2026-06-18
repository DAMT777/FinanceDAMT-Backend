using FinanceDAMT.Application.Features.Subscriptions.DTOs;
using FinanceDAMT.Domain.Enums;
using MediatR;

namespace FinanceDAMT.Application.Features.Subscriptions.Commands.CreateSubscription;

public sealed record CreateSubscriptionCommand(
    string Name,
    decimal Amount,
    BillingCycle BillingCycle,
    DateTime NextBillingDate,
    string Icon,
    string? Notes
) : IRequest<SubscriptionDto>;
