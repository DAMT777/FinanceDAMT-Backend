using FinanceDAMT.Application.Features.SavingGoals.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.SavingGoals.Commands.RemoveContribution;

public sealed record RemoveContributionCommand(Guid GoalId, Guid ContributionId) : IRequest<SavingGoalDto>;
