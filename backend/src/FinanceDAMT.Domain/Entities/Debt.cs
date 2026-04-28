using FinanceDAMT.Domain.Common;
using FinanceDAMT.Domain.Enums;

namespace FinanceDAMT.Domain.Entities;

public class Debt : BaseEntity
{
    public Guid UserId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsPaid { get; set; } = false;
    public string CreditorOrDebtor { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public DebtType Type { get; set; }

    public User User { get; set; } = null!;
}
