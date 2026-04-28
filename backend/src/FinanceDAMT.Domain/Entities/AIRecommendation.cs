using FinanceDAMT.Domain.Common;
using FinanceDAMT.Domain.Enums;

namespace FinanceDAMT.Domain.Entities;

public class AIRecommendation : BaseEntity
{
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public AIRecommendationType Type { get; set; }
    public bool? IsUseful { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;

    public User User { get; set; } = null!;
}
