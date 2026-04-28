using FinanceDAMT.Domain.Common;

namespace FinanceDAMT.Domain.Entities;

public class SavingContribution : BaseEntity
{
    public Guid GoalId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Note { get; set; }

    public SavingGoal Goal { get; set; } = null!;
}
