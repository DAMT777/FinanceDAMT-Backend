using FinanceDAMT.Domain.Common;

namespace FinanceDAMT.Domain.Entities;

public class Budget : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid CategoryId { get; set; }
    public decimal MonthlyLimit { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public bool AlertSent80 { get; set; }
    public bool AlertSent100 { get; set; }

    public User User { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
