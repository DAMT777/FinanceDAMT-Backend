using FinanceDAMT.Domain.Common;
using FinanceDAMT.Domain.Enums;

namespace FinanceDAMT.Domain.Entities;

public class Transaction : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid AccountId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public Guid CategoryId { get; set; }
    public string? Description { get; set; }
    public string? ReceiptUrl { get; set; }
    public bool IsRecurring { get; set; } = false;

    public User User { get; set; } = null!;
    public Account Account { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
