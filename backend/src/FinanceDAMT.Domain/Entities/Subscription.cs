using FinanceDAMT.Domain.Common;
using FinanceDAMT.Domain.Enums;

namespace FinanceDAMT.Domain.Entities;

public class Subscription : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;
    public DateTime NextBillingDate { get; set; }
    public string Icon { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public User User { get; set; } = null!;
}
