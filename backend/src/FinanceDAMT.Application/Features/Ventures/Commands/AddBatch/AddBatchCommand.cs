using FinanceDAMT.Application.Features.Ventures.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.Ventures.Commands.AddBatch;

public sealed record AddBatchCommand(
    Guid VentureId,
    string Label,
    DateTime Date,
    decimal Investment,
    int UnitsProduced,
    decimal UnitPrice,
    string? Notes
) : IRequest<VentureDto>;
