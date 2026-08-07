using FinanceDAMT.Application.Features.Ventures.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.Ventures.Queries.GetVentureById;

public sealed record GetVentureByIdQuery(Guid Id) : IRequest<VentureDto>;
