using FinanceDAMT.Application.Features.AI.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.AI.Commands.SendChatMessage;

public sealed record SendChatMessageCommand(string Message) : IRequest<ChatResponseDto>;
