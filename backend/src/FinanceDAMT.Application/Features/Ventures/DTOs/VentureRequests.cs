namespace FinanceDAMT.Application.Features.Ventures.DTOs;

public sealed record CreateVentureRequest(
    string Name,
    string Icon,
    string? Description
);

public sealed record UpdateVentureRequest(
    string Name,
    string Icon,
    string? Description,
    bool IsActive
);

public sealed record CreateBatchRequest(
    string Label,
    DateTime Date,
    decimal Investment,
    int UnitsProduced,
    decimal Income,
    string? Notes
);

public sealed record UpdateBatchRequest(
    string Label,
    DateTime Date,
    decimal Investment,
    int UnitsProduced,
    decimal Income,
    string? Notes
);
