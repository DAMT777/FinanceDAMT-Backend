using MediatR;

namespace FinanceDAMT.Application.Features.Ventures.Commands.DeleteVenture;

public sealed record DeleteVentureCommand(Guid Id) : IRequest<Unit>;
