using FinanceDAMT.Domain.Common;

namespace FinanceDAMT.Domain.Entities;

/// <summary>
/// A single production cycle of a <see cref="Venture"/>: the investment in
/// materials, the number of units produced from it, and the income earned from
/// selling that production. Profitability metrics are derived from these fields.
/// </summary>
public class VentureBatch : BaseEntity
{
    public Guid VentureId { get; set; }
    public string Label { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Investment { get; set; }
    public int UnitsProduced { get; set; }
    public decimal Income { get; set; }
    public string? Notes { get; set; }

    public Venture Venture { get; set; } = null!;
}
