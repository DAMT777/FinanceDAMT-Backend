using System.Text.Json;
using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.AI.DTOs;
using FinanceDAMT.Domain.Entities;
using MediatR;

namespace FinanceDAMT.Application.Features.AI.Commands.CalculateFinancialScore;

public sealed class CalculateFinancialScoreCommandHandler : IRequestHandler<CalculateFinancialScoreCommand, FinancialScoreDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAIService _aiService;

    public CalculateFinancialScoreCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IAIService aiService)
    {
        _context = context;
        _currentUser = currentUser;
        _aiService = aiService;
    }

    public async Task<FinancialScoreDto> Handle(CalculateFinancialScoreCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");
        var dto = await _aiService.CalculateFinancialScore(userId);

        var now = DateTime.UtcNow;
        _context.FinancialScores.Add(new FinancialScore
        {
            UserId = userId,
            Score = dto.Score,
            Month = now.Month,
            Year = now.Year,
            Breakdown = JsonSerializer.Serialize(dto)
        });

        await _context.SaveChangesAsync(cancellationToken);
        return dto;
    }
}
