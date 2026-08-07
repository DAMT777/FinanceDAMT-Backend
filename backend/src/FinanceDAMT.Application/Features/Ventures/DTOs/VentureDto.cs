namespace FinanceDAMT.Application.Features.Ventures.DTOs;

public sealed record VentureDto(
    Guid Id,
    string Name,
    string Icon,
    string? Description,
    bool IsActive,
    decimal TotalInvestment,
    int TotalUnitsProduced,
    decimal TotalIncome,
    decimal UnitCost,
    decimal NetBalance,
    decimal RoiPercentage,
    int BatchCount,
    IReadOnlyList<VentureBatchDto> Batches
);

public sealed record VentureBatchDto(
    Guid Id,
    string Label,
    DateTime Date,
    decimal Investment,
    int UnitsProduced,
    decimal Income,
    decimal UnitCost,
    decimal NetBalance,
    decimal RoiPercentage,
    string? Notes
);
