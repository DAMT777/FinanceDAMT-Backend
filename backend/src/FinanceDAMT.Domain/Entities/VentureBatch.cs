using FinanceDAMT.Domain.Common;

namespace FinanceDAMT.Domain.Entities;

/// <summary>
/// A single production cycle of a <see cref="Venture"/>: the investment in
/// materials, the units produced, the sale price per unit, and how many units
/// have been sold so far. Revenue and profitability metrics are derived from
/// these fields (revenue = UnitsSold * UnitPrice).
/// </summary>
public class VentureBatch : BaseEntity
{
    public Guid VentureId { get; set; }
    public string Label { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Investment { get; set; }
    public int UnitsProduced { get; set; }
    public decimal UnitPrice { get; set; }
    public int UnitsSold { get; set; }
    public string? Notes { get; set; }

    public Venture Venture { get; set; } = null!;
}
