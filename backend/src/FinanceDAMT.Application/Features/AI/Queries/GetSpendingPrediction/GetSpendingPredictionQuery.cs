using FinanceDAMT.Application.Features.AI.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.AI.Queries.GetSpendingPrediction;

public sealed record GetSpendingPredictionQuery : IRequest<SpendingPredictionDto>;
