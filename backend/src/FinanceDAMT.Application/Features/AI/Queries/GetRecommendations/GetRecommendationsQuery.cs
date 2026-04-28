using MediatR;

namespace FinanceDAMT.Application.Features.AI.Queries.GetRecommendations;

public sealed record GetRecommendationsQuery : IRequest<IReadOnlyList<string>>;
