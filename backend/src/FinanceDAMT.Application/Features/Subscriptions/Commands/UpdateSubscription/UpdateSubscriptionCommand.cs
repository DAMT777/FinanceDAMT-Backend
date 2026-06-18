using FinanceDAMT.Application.Features.Subscriptions.DTOs;
using FinanceDAMT.Domain.Enums;
using MediatR;

namespace FinanceDAMT.Application.Features.Subscriptions.Commands.UpdateSubscription;

public sealed record UpdateSubscriptionCommand(
    Guid Id,
    string Name,
    decimal Amount,
    BillingCycle BillingCycle,
    DateTime NextBillingDate,
    string Icon,
    bool IsActive,
    string? Notes
) : IRequest<SubscriptionDto>;
