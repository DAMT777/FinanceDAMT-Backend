using FinanceDAMT.Application.Features.AI.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.AI.Queries.GetChatHistory;

public sealed record GetChatHistoryQuery : IRequest<IReadOnlyList<ChatMessageDto>>;
