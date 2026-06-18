using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.AI.Agent;
using FinanceDAMT.Application.Features.AI.Commands.LogExpenseFromText;
using FinanceDAMT.Application.Features.AI.DTOs;
using FinanceDAMT.Application.Features.AI.Queries.GenerateReport;
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
    private readonly ISender _sender;

    public SendChatMessageCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IAIService aiService,
        ISender sender)
    {
        _context = context;
        _currentUser = currentUser;
        _aiService = aiService;
        _sender = sender;
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

        // ── Agent routing: report request → log statement → general chat ──────────
        var responseText = await ResolveAgentResponseAsync(userId, request.Message, history, cancellationToken);

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
            Content = $"ASSISTANT:{responseText}",
            GeneratedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var updatedHistory = history
            .Append(new ChatMessageDto("user", request.Message, now))
            .Append(new ChatMessageDto("assistant", responseText, now))
            .ToList();

        return new ChatResponseDto(responseText, updatedHistory);
    }

    private async Task<string> ResolveAgentResponseAsync(
        Guid userId, string message, List<ChatMessageDto> history, CancellationToken cancellationToken)
    {
        var reportPeriod = FinanceTextParser.TryParseReportPeriod(message);
        if (reportPeriod is not null)
        {
            var report = await _sender.Send(new GenerateReportQuery(reportPeriod.Value), cancellationToken);
            return report.Message;
        }

        if (FinanceTextParser.TryParseExpense(message) is not null)
        {
            var result = await _sender.Send(new LogExpenseFromTextCommand(message), cancellationToken);
            return result.Message;
        }

        var aiResponse = await _aiService.ChatAsync(userId, message, history);
        return aiResponse.Response;
    }
}
