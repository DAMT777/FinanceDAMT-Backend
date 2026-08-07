using FinanceDAMT.Application.Features.Ventures.DTOs;
using FinanceDAMT.Domain.Entities;

namespace FinanceDAMT.Application.Features.Ventures;

/// <summary>
/// Maps ventures/batches to their DTOs and derives the profitability metrics
/// (unit cost, revenue, net balance, ROI %) both per batch and aggregated per
/// venture. Revenue = UnitsSold * UnitPrice. Metrics are computed, never stored.
/// </summary>
internal static class VentureProjection
{
    public static decimal UnitCost(decimal investment, int units)
        => units > 0 ? Math.Round(investment / units, 2) : 0m;

    public static decimal Roi(decimal investment, decimal revenue)
        => investment > 0 ? Math.Round((revenue - investment) / investment * 100m, 2) : 0m;

    public static VentureBatchDto ToDto(VentureBatch b)
    {
        var revenue = Math.Round(b.UnitsSold * b.UnitPrice, 2);
        return new VentureBatchDto(
            b.Id,
            b.Label,
            b.Date,
            b.Investment,
            b.UnitsProduced,
            b.UnitPrice,
            b.UnitsSold,
            Math.Max(0, b.UnitsProduced - b.UnitsSold),
            revenue,
            UnitCost(b.Investment, b.UnitsProduced),
            revenue - b.Investment,
            Roi(b.Investment, revenue),
            b.Notes);
    }

    public static VentureDto ToDto(Venture v)
    {
        var batches = v.Batches.Where(b => !b.IsDeleted).OrderByDescending(b => b.Date).ToList();
        var totalInvestment = batches.Sum(b => b.Investment);
        var totalUnits = batches.Sum(b => b.UnitsProduced);
        var totalSold = batches.Sum(b => b.UnitsSold);
        var totalRevenue = batches.Sum(b => Math.Round(b.UnitsSold * b.UnitPrice, 2));

        return new VentureDto(
            v.Id,
            v.Name,
            v.Icon,
            v.Description,
            v.IsActive,
            totalInvestment,
            totalUnits,
            totalSold,
            Math.Max(0, totalUnits - totalSold),
            totalRevenue,
            UnitCost(totalInvestment, totalUnits),
            totalRevenue - totalInvestment,
            Roi(totalInvestment, totalRevenue),
            batches.Count,
            batches.Select(ToDto).ToList());
    }
}
