using FinanceDAMT.Domain.Common;

namespace FinanceDAMT.Domain.Entities;

public class SavingGoal : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public DateTime Deadline { get; set; }
    public string Icon { get; set; } = string.Empty;
    public bool IsCompleted { get; set; } = false;
    public int MilestonesReached { get; set; }

    public User User { get; set; } = null!;
    public ICollection<SavingContribution> Contributions { get; set; } = new List<SavingContribution>();
}
