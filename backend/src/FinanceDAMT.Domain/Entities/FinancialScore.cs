using FinanceDAMT.Domain.Common;

namespace FinanceDAMT.Domain.Entities;

public class FinancialScore : BaseEntity
{
    public Guid UserId { get; set; }
    public int Score { get; set; }
    public string Breakdown { get; set; } = "{}";
    public int Month { get; set; }
    public int Year { get; set; }

    public User User { get; set; } = null!;
}
