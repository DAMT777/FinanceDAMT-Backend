using FinanceDAMT.Domain.Enums;

namespace FinanceDAMT.Application.Features.Subscriptions.DTOs;

public sealed record CreateSubscriptionRequest(
    string Name,
    decimal Amount,
    BillingCycle BillingCycle,
    DateTime NextBillingDate,
    string Icon,
    string? Notes
);
