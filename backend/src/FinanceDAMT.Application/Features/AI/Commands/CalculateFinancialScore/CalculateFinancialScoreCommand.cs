using FinanceDAMT.Application.Features.AI.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.AI.Commands.CalculateFinancialScore;

public sealed record CalculateFinancialScoreCommand : IRequest<FinancialScoreDto>;
