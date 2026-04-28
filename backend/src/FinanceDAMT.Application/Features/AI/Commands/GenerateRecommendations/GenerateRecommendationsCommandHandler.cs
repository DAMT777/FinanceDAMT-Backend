using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Domain.Entities;
using FinanceDAMT.Domain.Enums;
using MediatR;

namespace FinanceDAMT.Application.Features.AI.Commands.GenerateRecommendations;

public sealed class GenerateRecommendationsCommandHandler : IRequestHandler<GenerateRecommendationsCommand, IReadOnlyList<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAIService _aiService;

    public GenerateRecommendationsCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IAIService aiService)
    {
        _context = context;
        _currentUser = currentUser;
        _aiService = aiService;
    }

    public async Task<IReadOnlyList<string>> Handle(GenerateRecommendationsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");
        var recommendations = await _aiService.GenerateRecommendations(userId);

        foreach (var item in recommendations)
        {
            _context.AIRecommendations.Add(new AIRecommendation
            {
                UserId = userId,
                Type = AIRecommendationType.Proactive,
                Content = item,
                GeneratedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return recommendations;
    }
}
