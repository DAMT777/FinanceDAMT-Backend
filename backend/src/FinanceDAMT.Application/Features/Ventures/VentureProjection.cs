using FinanceDAMT.Application.Features.Ventures.DTOs;
using FinanceDAMT.Domain.Entities;

namespace FinanceDAMT.Application.Features.Ventures;

/// <summary>
/// Maps ventures/batches to their DTOs and derives the profitability metrics
/// (unit cost, net balance, ROI %) both per batch and aggregated per venture.
/// Metrics are computed, never stored.
/// </summary>
internal static class VentureProjection
{
    public static decimal UnitCost(decimal investment, int units)
        => units > 0 ? Math.Round(investment / units, 2) : 0m;

    public static decimal Roi(decimal investment, decimal income)
        => investment > 0 ? Math.Round((income - investment) / investment * 100m, 2) : 0m;

    public static VentureBatchDto ToDto(VentureBatch b) => new(
        b.Id,
        b.Label,
        b.Date,
        b.Investment,
        b.UnitsProduced,
        b.Income,
        UnitCost(b.Investment, b.UnitsProduced),
        b.Income - b.Investment,
        Roi(b.Investment, b.Income),
        b.Notes);

    public static VentureDto ToDto(Venture v)
    {
        var batches = v.Batches.Where(b => !b.IsDeleted).OrderByDescending(b => b.Date).ToList();
        var totalInvestment = batches.Sum(b => b.Investment);
        var totalUnits = batches.Sum(b => b.UnitsProduced);
        var totalIncome = batches.Sum(b => b.Income);

        return new VentureDto(
            v.Id,
            v.Name,
            v.Icon,
            v.Description,
            v.IsActive,
            totalInvestment,
            totalUnits,
            totalIncome,
            UnitCost(totalInvestment, totalUnits),
            totalIncome - totalInvestment,
            Roi(totalInvestment, totalIncome),
            batches.Count,
            batches.Select(ToDto).ToList());
    }
}
