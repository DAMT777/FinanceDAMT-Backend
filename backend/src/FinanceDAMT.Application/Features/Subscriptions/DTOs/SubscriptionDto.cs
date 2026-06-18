using FinanceDAMT.Domain.Enums;

namespace FinanceDAMT.Application.Features.Subscriptions.DTOs;

public sealed record SubscriptionDto(
    Guid Id,
    string Name,
    decimal Amount,
    BillingCycle BillingCycle,
    DateTime NextBillingDate,
    string Icon,
    bool IsActive,
    string? Notes,
    decimal MonthlyCost
);
