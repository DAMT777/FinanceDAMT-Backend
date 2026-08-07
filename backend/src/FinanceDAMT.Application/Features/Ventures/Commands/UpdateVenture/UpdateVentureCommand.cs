using FinanceDAMT.Application.Features.Ventures.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.Ventures.Commands.UpdateVenture;

public sealed record UpdateVentureCommand(
    Guid Id,
    string Name,
    string Icon,
    string? Description,
    bool IsActive
) : IRequest<VentureDto>;
