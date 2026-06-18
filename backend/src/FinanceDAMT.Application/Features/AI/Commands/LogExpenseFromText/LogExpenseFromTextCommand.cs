using FinanceDAMT.Application.Features.AI.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.AI.Commands.LogExpenseFromText;

public sealed record LogExpenseFromTextCommand(string Text) : IRequest<AgentLogResultDto>;
