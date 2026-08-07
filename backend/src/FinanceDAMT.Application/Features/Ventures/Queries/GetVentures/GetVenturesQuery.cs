using FinanceDAMT.Application.Features.Ventures.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.Ventures.Queries.GetVentures;

public sealed record GetVenturesQuery() : IRequest<IReadOnlyList<VentureDto>>;
