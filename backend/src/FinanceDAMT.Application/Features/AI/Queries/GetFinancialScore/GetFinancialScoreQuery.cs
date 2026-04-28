using FinanceDAMT.Application.Features.AI.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.AI.Queries.GetFinancialScore;

public sealed record GetFinancialScoreQuery : IRequest<FinancialScoreDto>;
