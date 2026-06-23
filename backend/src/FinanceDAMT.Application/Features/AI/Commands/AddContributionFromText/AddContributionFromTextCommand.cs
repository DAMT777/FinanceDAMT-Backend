using FinanceDAMT.Application.Features.AI.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.AI.Commands.AddContributionFromText;

public sealed record AddContributionFromTextCommand(string Text) : IRequest<AgentLogResultDto>;
