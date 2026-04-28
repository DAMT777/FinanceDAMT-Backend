namespace FinanceDAMT.Application.Features.AI.DTOs;

public sealed record ChatMessageDto(string Role, string Content, DateTime Timestamp);

public sealed record ChatResponseDto(string Response, IReadOnlyList<ChatMessageDto> History);
