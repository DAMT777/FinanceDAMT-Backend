using FinanceDAMT.Application.Features.Ventures.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.Ventures.Commands.CreateVenture;

public sealed record CreateVentureCommand(
    string Name,
    string Icon,
    string? Description
) : IRequest<VentureDto>;
