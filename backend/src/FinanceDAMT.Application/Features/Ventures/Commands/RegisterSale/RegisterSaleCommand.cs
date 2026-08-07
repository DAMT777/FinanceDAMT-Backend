using FinanceDAMT.Application.Features.Ventures.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.Ventures.Commands.RegisterSale;

public sealed record RegisterSaleCommand(
    Guid VentureId,
    Guid BatchId,
    int Units
) : IRequest<VentureDto>;
