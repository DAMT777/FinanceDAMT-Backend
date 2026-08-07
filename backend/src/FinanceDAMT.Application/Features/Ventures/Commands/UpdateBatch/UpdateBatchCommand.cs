using FinanceDAMT.Application.Features.Ventures.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.Ventures.Commands.UpdateBatch;

public sealed record UpdateBatchCommand(
    Guid VentureId,
    Guid BatchId,
    string Label,
    DateTime Date,
    decimal Investment,
    int UnitsProduced,
    decimal Income,
    string? Notes
) : IRequest<VentureDto>;
