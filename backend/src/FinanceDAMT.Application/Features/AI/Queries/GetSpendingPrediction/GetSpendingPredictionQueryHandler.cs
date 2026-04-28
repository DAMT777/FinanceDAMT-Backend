using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.AI.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.AI.Queries.GetSpendingPrediction;

public sealed class GetSpendingPredictionQueryHandler : IRequestHandler<GetSpendingPredictionQuery, SpendingPredictionDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IAIService _aiService;

    public GetSpendingPredictionQueryHandler(ICurrentUserService currentUser, IAIService aiService)
    {
        _currentUser = currentUser;
        _aiService = aiService;
    }

    public async Task<SpendingPredictionDto> Handle(GetSpendingPredictionQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");
        return await _aiService.PredictNextMonth(userId);
    }
}
