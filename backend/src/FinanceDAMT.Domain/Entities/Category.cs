using FinanceDAMT.Domain.Common;
using FinanceDAMT.Domain.Enums;

namespace FinanceDAMT.Domain.Entities;

public class Category : BaseEntity
{
    /// <summary>Null = global default category; non-null = user-defined category.</summary>
    public Guid? UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = "#6B7280";
    public CategoryType Type { get; set; }

    public User? User { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
}
