using FinanceDAMT.Application.Features.SavingGoals.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.SavingGoals.Commands.AddContribution;

public sealed record AddContributionCommand(
    Guid GoalId,
    decimal Amount,
    DateTime Date,
    string? Note
) : IRequest<SavingGoalDto>;
