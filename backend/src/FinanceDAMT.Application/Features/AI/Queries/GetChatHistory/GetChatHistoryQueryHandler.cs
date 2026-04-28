using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.AI.DTOs;
using FinanceDAMT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.AI.Queries.GetChatHistory;

public sealed class GetChatHistoryQueryHandler : IRequestHandler<GetChatHistoryQuery, IReadOnlyList<ChatMessageDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetChatHistoryQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<ChatMessageDto>> Handle(GetChatHistoryQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var rawHistory = await _context.AIRecommendations
            .AsNoTracking()
            .Where(r => r.UserId == userId && r.Type == AIRecommendationType.Chat)
            .OrderByDescending(r => r.GeneratedAt)
            .Take(20)
            .OrderBy(r => r.GeneratedAt)
            .Select(r => new { r.Content, r.GeneratedAt })
            .ToListAsync(cancellationToken);

        return rawHistory
            .Select(r => new ChatMessageDto(
                r.Content.StartsWith("USER:") ? "user" : "assistant",
                r.Content.StartsWith("USER:") ? r.Content[5..] : r.Content[10..],
                r.GeneratedAt))
            .ToList();
    }
}
