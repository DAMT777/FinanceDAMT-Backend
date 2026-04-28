using System.Text.Json;
using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.AI.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.AI.Queries.GetFinancialScore;

public sealed class GetFinancialScoreQueryHandler : IRequestHandler<GetFinancialScoreQuery, FinancialScoreDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetFinancialScoreQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<FinancialScoreDto> Handle(GetFinancialScoreQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var latest = await _context.FinancialScores
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.Year)
            .ThenByDescending(f => f.Month)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Financial score not found.");

        return JsonSerializer.Deserialize<FinancialScoreDto>(latest.Breakdown) ??
               new FinancialScoreDto(latest.Score, 0, 0, 0, 0, "No detailed breakdown available.");
    }
}
