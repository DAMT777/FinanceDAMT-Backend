using MediatR;

namespace FinanceDAMT.Application.Features.AI.Commands.GenerateRecommendations;

public sealed record GenerateRecommendationsCommand : IRequest<IReadOnlyList<string>>;
