using FinanceDAMT.Application.Features.Subscriptions.DTOs;
using FinanceDAMT.Domain.Entities;
using FinanceDAMT.Domain.Enums;

namespace FinanceDAMT.Application.Features.Subscriptions;

internal static class SubscriptionProjection
{
        public static decimal CalculateMonthlyCost(decimal amount, BillingCycle cycle)
    {
        var monthly = cycle switch
        {
            BillingCycle.Weekly => amount * 52m / 12m,
            BillingCycle.Monthly => amount,
            BillingCycle.Quarterly => amount / 3m,
            BillingCycle.Yearly => amount / 12m,
            _ => amount
        };

        return Math.Round(monthly, 2);
    }

    public static SubscriptionDto ToDto(Subscription subscription) => new(
        subscription.Id,
        subscription.Name,
        subscription.Amount,
        subscription.BillingCycle,
        subscription.NextBillingDate,
        subscription.Icon,
        subscription.IsActive,
        subscription.Notes,
        CalculateMonthlyCost(subscription.Amount, subscription.BillingCycle));
}
