using FinanceDAMT.Application.Features.Ventures.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.Ventures.Commands.DeleteBatch;

public sealed record DeleteBatchCommand(Guid VentureId, Guid BatchId) : IRequest<VentureDto>;
