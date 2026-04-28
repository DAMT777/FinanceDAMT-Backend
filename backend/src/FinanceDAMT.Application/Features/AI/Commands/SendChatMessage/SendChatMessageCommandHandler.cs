using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.AI.DTOs;
using FinanceDAMT.Domain.Entities;
using FinanceDAMT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.AI.Commands.SendChatMessage;

public sealed class SendChatMessageCommandHandler : IRequestHandler<SendChatMessageCommand, ChatResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAIService _aiService;

    public SendChatMessageCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IAIService aiService)
    {
        _context = context;
        _currentUser = currentUser;
        _aiService = aiService;
    }

    public async Task<ChatResponseDto> Handle(SendChatMessageCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var rawHistory = await _context.AIRecommendations
            .Where(r => r.UserId == userId && r.Type == AIRecommendationType.Chat)
            .OrderByDescending(r => r.GeneratedAt)
            .Take(20)
            .OrderBy(r => r.GeneratedAt)
            .Select(r => new { r.Content, r.GeneratedAt })
            .ToListAsync(cancellationToken);

        var history = rawHistory
            .Select(r => new ChatMessageDto(
                r.Content.StartsWith("USER:") ? "user" : "assistant",
                r.Content.StartsWith("USER:") ? r.Content[5..] : r.Content[10..],
                r.GeneratedAt))
            .ToList();

        var response = await _aiService.ChatAsync(userId, request.Message, history);

        _context.AIRecommendations.Add(new AIRecommendation
        {
            UserId = userId,
            Type = AIRecommendationType.Chat,
            Content = $"USER:{request.Message}",
            GeneratedAt = DateTime.UtcNow
        });

        _context.AIRecommendations.Add(new AIRecommendation
        {
            UserId = userId,
            Type = AIRecommendationType.Chat,
            Content = $"ASSISTANT:{response.Response}",
            GeneratedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
        return response;
    }
}
