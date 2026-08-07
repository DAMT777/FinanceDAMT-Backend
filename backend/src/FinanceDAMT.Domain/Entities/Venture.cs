using FinanceDAMT.Domain.Common;

namespace FinanceDAMT.Domain.Entities;

/// <summary>
/// An entrepreneurship project the user tracks for profitability (e.g. cookie
/// sales). A venture aggregates one or more production batches.
/// </summary>
public class Venture : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public User User { get; set; } = null!;
    public ICollection<VentureBatch> Batches { get; set; } = new List<VentureBatch>();
}
